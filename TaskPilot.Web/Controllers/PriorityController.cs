using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class PriorityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public PriorityController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            if (claim != null)
            {
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
                var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

                foreach (var permission in permissions)
                {
                    if (permission.Roles.Any(r => r.Id == roles.Id))
                    {
                        viewModel.UserPermissions.Add(permission);
                    }
                }

            }

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
                        Description = viewModel.Name,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    _unitOfWork.Priority.Add(priority);
                    TempData["SuccessMsg"] = "A new priority has been created";
                }
                else
                {
                    Priorities priorityToEdit = _unitOfWork.Priority.Get(p => p.Id == viewModel.Id);
                    priorityToEdit.Description = viewModel.Name;
                    priorityToEdit.UpdatedAt = DateTime.Now;

                    _unitOfWork.Priority.Update(priorityToEdit);
                    TempData["SuccessMsg"] = priorityToEdit.Description + "'s Priority has been updated.";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Priority");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var priorityInDb = _unitOfWork.Priority.Get(p => p.Description == name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = priorityInDb.Id,
                Name = priorityInDb.Description,
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
                    priorityToDelete.Add(_unitOfWork.Priority.Get(p => p.Id == priorityId));
                }
            }

            if (priorityToDelete != null)
            {
                foreach (var priorities in priorityToDelete)
                {
                    if (_unitOfWork.Tasks.GetAll().Any(s => s.PriorityId == priorities.Id))
                    {
                        TempData["ErrorMsg"] = "You can't delete a priority that is being used by any task currently.";
                        return Json(Url.Action("Index", "Priority"));
                    }
                    else
                    {
                        _unitOfWork.Priority.Remove(priorities);
                    }
                }
            }

            _unitOfWork.Save();
            TempData["SuccessMsg"] = priority.Length + " priority has been deleted successfully";
            return Json(Url.Action("Index", "Priority"));
        }

    }
}
