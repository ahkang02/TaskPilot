using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Abstractions;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class StatusController : Controller
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly IStatusService _statusService;

        public StatusController(IStatusService statusService, IUserPermissionService userPermissionService)
        {
            _statusService = statusService;
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
            EditStatusViewModel viewModel = new EditStatusViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(EditStatusViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Statuses status = new Statuses
                    {
                        ColorCode = viewModel.ColorCode!,
                        Description = viewModel.Name!,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    _statusService.CreateStatus(status);
                    TempData["SuccessMsg"] = Message.STAT_CREATION;
                }
                else
                {
                    Statuses statusToEdit = _statusService.GetStatusById(viewModel.Id.Value);
                    statusToEdit.Description = viewModel.Name!;
                    statusToEdit.UpdatedAt = DateTime.Now;
                    statusToEdit.ColorCode = viewModel.ColorCode!;

                    _statusService.UpdateStatus(statusToEdit);
                    TempData["SuccessMsg"] = statusToEdit.Description + Message.STAT_UPDATE;
                }
                return RedirectToAction("Index", "Status");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var statusInDb = _statusService.GetStatusByName(name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Description,
                ColorCode = statusInDb.ColorCode != null ? statusInDb.ColorCode : "",
            };

            return View("New", viewModel);
        }

        public IActionResult Delete(Guid[] status)
        {
            var statusToDelete = new List<Statuses>();
            if (status.Length > 0)
            {
                for (int i = 0; i < status.Length; i++)
                {
                    var statusId = status[i];
                    statusToDelete.Add(_statusService.GetStatusById(statusId));
                }

                if (statusToDelete != null)
                {
                    foreach (var statuses in statusToDelete)
                    {
                        if (_statusService.CheckIfStatusIsInUse(statuses))
                        {
                            TempData["ErrorMsg"] = Message.STAT_DELETION_FAIL;
                            return Json(Url.Action("Index", "Status"));
                        }
                        else
                        {
                            _statusService.DeleteStatus(statuses);
                        }
                    }
                }

            }
            TempData["SuccessMsg"] = status.Length + Message.STAT_DELETION;
            return Json(Url.Action("Index", "Status"));
        }
    }

}
