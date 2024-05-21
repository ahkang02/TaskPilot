using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Abstractions;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class StatusController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatusController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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
                        Description = viewModel.Name,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    _unitOfWork.Status.Add(status);
                    TempData["SuccessMsg"] = "A new status has been created";
                }
                else
                {
                    Statuses statusToEdit = _unitOfWork.Status.Get(s => s.Id == viewModel.Id);
                    statusToEdit.Description = viewModel.Name;
                    statusToEdit.UpdatedAt = DateTime.Now;

                    _unitOfWork.Status.Update(statusToEdit);
                    TempData["SuccessMsg"] = statusToEdit.Description + "'s Status has been updated.";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Status");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var statusInDb = _unitOfWork.Status.Get(s => s.Description == name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Description,
            };

            return View("New", viewModel);
        }

        public IActionResult Delete(string[] status)
        {
            var statusToDelete = new List<Statuses>();
            if (status.Length > 0)
            {
                for (int i = 0; i < status.Length; i++)
                {
                    var statusName = status[i];
                    statusToDelete.Add(_unitOfWork.Status.Get(s => s.Description ==  statusName));
                }

                if (statusToDelete != null)
                {
                    _unitOfWork.Status.RemoveRange(statusToDelete);
                }

            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = status.Length + " status has been deleted successfully";
            return Json(Url.Action("Index", "Status"));
        }
    }

}
