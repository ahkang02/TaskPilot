using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
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
                    TempData["SuccessMsg"] = Message.PERMIT_CREATION;
                    _unitOfWork.Permissions.Add(permission);
                }
                else
                {
                    Permission permissionToEdit = _unitOfWork.Permissions.Get(p => p.Name == viewModel.Name);
                    permissionToEdit.Name = viewModel.Name;
                    permissionToEdit.UpdatedAt = DateTime.Now;

                    _unitOfWork.Permissions.Update(permissionToEdit);
                    TempData["SuccessMsg"] = permissionToEdit.Name + Message.PERMIT_UPDATE;

                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Permission");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return View(viewModel);
        }
    }
}
