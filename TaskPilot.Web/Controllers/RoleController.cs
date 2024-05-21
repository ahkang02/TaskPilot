using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class RoleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;


        public RoleController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
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
            EditRoleViewModel viewModel = new EditRoleViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(EditRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    ApplicationRole roles = new ApplicationRole
                    {
                        Name = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _roleManager.CreateAsync(roles);
                    TempData["SuccessMsg"] = "A new role has been created";
                }
                else
                {
                    ApplicationRole? rolesToEdit = await _roleManager.FindByIdAsync(viewModel.Id);

                    if (rolesToEdit != null)
                    {
                        rolesToEdit.Name = viewModel.Name;
                        rolesToEdit.UpdatedAt = DateTime.Now;
                        await _roleManager.UpdateAsync(rolesToEdit);
                        TempData["SuccessMsg"] = rolesToEdit.Name + "'s Roles has been updated.";
                    }
                }
                return RedirectToAction("Index", "Role");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var roleInDB = _unitOfWork.Roles.Get(r => r.Name == name);
            EditRoleViewModel viewModel = new EditRoleViewModel
            {
                Id = roleInDB.Id,
                Name = roleInDB.Name
            };

            return View("New", viewModel);
        }

        public async Task<IActionResult> Delete(string[] roleName)
        {
            var roleToDelete = new List<ApplicationRole>();
            if (roleName.Length > 0)
            {
                for (int i = 0; i < roleName.Length; i++)
                {
                    var role = roleName[i];
                    roleToDelete.Add(_unitOfWork.Roles.Get(r => r.Name == role));
                }

                foreach (var role in roleToDelete)
                {
                    var users = _unitOfWork.Users.GetAll();
                    foreach (var user in users)
                    {
                        bool flag = await _userManager.IsInRoleAsync(user, role.Name);
                        if (flag)
                        {
                            TempData["ErrorMsg"] = "There are user exist in the role, you can't delete a role when there's user exist.";
                            return BadRequest(new {data = Url.Action("Index", "Role")});
                        }else
                        {
                            _unitOfWork.Roles.Remove(role);
                        }
                    }
                }
            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = roleName.Length + " roles deleted successfully";
            return Json(Url.Action("Index", "Role"));
        }

        public IActionResult AssignPermission(string name)
        {
            var role = _unitOfWork.Roles.GetAllInclude(r => r.Name == name, "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(null, "Features").OrderBy(r => r.Features.Name).ToList();
            var features = _unitOfWork.Features.GetAll().Select(r => r.Name);

            AssignPermissionViewModel viewModel = new AssignPermissionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsActive = role.IsActive,
                FeaturePermissions = new List<FeaturePermissionViewModel>()
            };

            foreach (var feature in features)
            {
                viewModel.FeaturePermissions.Add(new FeaturePermissionViewModel
                {
                    FeatureName = feature
                });
            }

            foreach (var featurePermission in viewModel.FeaturePermissions)
            {
                featurePermission.Permissions = new List<PermissionSelectViewModel>();

                foreach (var permission in permissions)
                {
                    if (featurePermission.FeatureName == permission.Features.Name)
                    {
                        featurePermission.Permissions.Add(new PermissionSelectViewModel
                        {
                            IsSelected = false,
                            Name = permission.Name,
                            PermissionId = permission.Id,
                        });
                    }
                }
            }

            foreach (var fp in viewModel.FeaturePermissions)
            {
                foreach (var p in fp.Permissions)
                {
                    foreach (var rp in role.Permissions)
                    {
                        if (rp.Id == p.PermissionId)
                        {
                            p.IsSelected = true;
                        }
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignPermission(AssignPermissionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                List<Permission> permissions = new List<Permission>();
                foreach (var features in viewModel.FeaturePermissions)
                {
                    foreach (var permission in features.Permissions ?? new List<PermissionSelectViewModel>())
                    {
                        if (permission != null && permission.IsSelected)
                        {
                            permissions.Add(_unitOfWork.Permissions.Get(p => p.Id == permission.PermissionId));
                        }
                    }
                }

                List<Permission> permissionsToRemoved = new List<Permission>();
                ApplicationRole roleToEdit = _unitOfWork.Roles.Get(r => r.Id == viewModel.RoleId);
                roleToEdit.UpdatedAt = DateTime.Now;
                var permissionInRole = _unitOfWork.Permissions.GetAllInclude(null, "Roles").ToList().Where(r => r.Roles == roleToEdit);

                foreach (var fp in viewModel.FeaturePermissions)
                {
                    foreach (var permission in fp.Permissions ?? new List<PermissionSelectViewModel>())
                    {
                        if (permission != null && !permission.IsSelected)
                        {
                            permissionsToRemoved.Add(_unitOfWork.Permissions.Get(p => p.Id == permission.PermissionId));
                        }
                    }
                }

                foreach (var p in permissionsToRemoved)
                {
                    p.Roles.Remove(roleToEdit);
                    _unitOfWork.Permissions.Update(p);
                }

                roleToEdit.Permissions = permissions;
                _unitOfWork.Roles.Update(roleToEdit);
                _unitOfWork.Save();
                TempData["SuccessMsg"] = "Role '" + roleToEdit.Name + "' permission has been updated";
                return RedirectToAction("Index", "Role");

            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

    }
}
