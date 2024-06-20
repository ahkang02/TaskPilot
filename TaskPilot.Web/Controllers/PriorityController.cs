using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class PriorityController : Controller
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly IPriorityService _priorityService;

        public PriorityController(IPriorityService priorityService, IUserPermissionService userPermissionService)
        {
            _priorityService = priorityService;
            _userPermissionService = userPermissionService;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = _userPermissionService.GetUserPermission(claimsIdentity).ToList()
            };
            return View(viewModel);
        }

        public IActionResult New()
        {
            EditPriorityViewModel viewModel = new EditPriorityViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(EditPriorityViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Priorities priority = new Priorities
                    {
                        ColorCode = viewModel.ColorCode!,
                        Description = viewModel.Name!,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    _priorityService.CreatePriority(priority);
                    TempData["SuccessMsg"] = Message.PRIOR_CREATION;
                }
                else
                {
                    Priorities priorityToEdit = _priorityService.GetPrioritiesById(viewModel.Id.Value);
                    priorityToEdit.Description = viewModel.Name!;
                    priorityToEdit.UpdatedAt = DateTime.Now;
                    priorityToEdit.ColorCode = viewModel.ColorCode!;

                    _priorityService.UpdatePriority(priorityToEdit);
                    TempData["SuccessMsg"] = priorityToEdit.Description + Message.PRIOR_UPDATE;
                }
                return RedirectToAction("Index", "Priority");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var priorityInDb = _priorityService.GetPrioritiesByName(name);
            EditPriorityViewModel viewModel = new EditPriorityViewModel
            {
                Id = priorityInDb.Id,
                Name = priorityInDb.Description,
                ColorCode = priorityInDb.ColorCode != null ? priorityInDb.ColorCode : ""
            };

            return View("New", viewModel);
        }

        public IActionResult Delete(Guid[] priority)
        {
            var priorityToDelete = new List<Priorities>();
            if (priority.Length > 0)
            {
                for (int i = 0; i < priority.Length; i++)
                {
                    var priorityId = priority[i];
                    priorityToDelete.Add(_priorityService.GetPrioritiesById(priorityId));
                }
            }

            if (priorityToDelete.Any())
            {
                foreach (var priorities in priorityToDelete)
                {
                    if (_priorityService.CheckIfPriorityIsInUse(priorities))
                    {
                        TempData["ErrorMsg"] = Message.PRIOR_DELETION_FAIL;
                        return Json(Url.Action("Index", "Priority"));
                    }
                    else
                    {
                        _priorityService.DeletePriority(priorities);
                    }
                }
            }
            TempData["SuccessMsg"] = priority.Length + Message.PRIOR_DELETION;
            return Json(Url.Action("Index", "Priority"));
        }

    }
}
