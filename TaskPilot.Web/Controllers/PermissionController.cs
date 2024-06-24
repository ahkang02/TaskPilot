using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IFeatureService _featureService;

        public PermissionController(IPermissionService permissionService, IFeatureService featureService)
        {
            _permissionService = permissionService;
            _featureService = featureService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult New()
        {
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Features = _featureService.GetAllFeatures().ToList()
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
                        Name = viewModel.Name!,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        FeaturesId = viewModel.FeatureId,
                        Features = _featureService.GetFeaturesById(viewModel.FeatureId)
                    };
                    _permissionService.CreatePermission(permission);
                    TempData["SuccessMsg"] = Message.PERMIT_CREATION;
                }
                else
                {
                    Permission permissionToEdit = _permissionService.GetPermissionById(viewModel.Id.Value);
                    permissionToEdit.Name = viewModel.Name!;
                    permissionToEdit.UpdatedAt = DateTime.Now;

                    _permissionService.UpdatePermission(permissionToEdit);
                    TempData["SuccessMsg"] = permissionToEdit.Name + Message.PERMIT_UPDATE;

                }
                return RedirectToAction("Index", "Permission");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return View(viewModel);
        }
    }
}
