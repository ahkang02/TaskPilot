using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult New()
        {
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Features = _unitOfWork.Features.GetAll().ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(EditPermissionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Permission permission = new Permission
                    {
                        Name = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        FeaturesId = viewModel.FeatureId,
                    };
                    TempData["SuccessMsg"] = "A new permission has been created";
                    _unitOfWork.Permissions.Add(permission);
                }
                else
                {
                    Permission permissionToEdit = _unitOfWork.Permissions.Get(p => p.Name == viewModel.Name);
                    permissionToEdit.Name = viewModel.Name;
                    permissionToEdit.UpdatedAt = DateTime.Now;

                    _unitOfWork.Permissions.Update(permissionToEdit);
                    TempData["SuccessMsg"] = permissionToEdit.Name + "'s permission has been updated";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Permission");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }
    }
}
