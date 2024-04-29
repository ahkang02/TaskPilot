using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Controllers
{
    public class RoleController : Controller
    {
        private TaskContext _context;
        private PermissionRepository _permissionRepository;
        private FeaturesRepository _featuresRepository;

        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;

        public RoleController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _featuresRepository = new FeaturesRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
            _roleManager = new RoleManager<Roles>(_roleStore);
        }

        // GET: Role
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var currentUserRole = _userManager.GetRoles(currentUser)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == currentUserRole);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();


            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            foreach (var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return View(viewModel);
        }

        public ActionResult NewRole()
        {
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features").ToList();
            var features = _featuresRepository.GetAll().Select(r => r.Name);

            EditRoleViewModel viewModel = new EditRoleViewModel
            {
                FeaturePermissions = new List<FeaturePermission>()
            };

            foreach (var feature in features)
            {
                viewModel.FeaturePermissions.Add(new FeaturePermission
                {
                    FeatureName = feature
                });
            }

            foreach (var featurePermission in viewModel.FeaturePermissions)
            {
                featurePermission.Permissions = new List<PermissionSelectViewModel>();

                foreach (var permission in permissions)
                {
                    if (featurePermission.FeatureName == permission.features.Name)
                    {
                        featurePermission.Permissions.Add(new PermissionSelectViewModel
                        {
                            Name = permission.Name,
                            PermissionId = permission.Id,
                            FeaturesName = featurePermission.FeatureName
                        });
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewRole(EditRoleViewModel viewModel)
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
                            permissions.Add(_permissionRepository.GetById(permission.PermissionId));
                        }
                    }
                }

                if (viewModel.RoleId == null)
                {
                    Roles role = new Roles
                    {
                        IsActive = true,
                        Name = viewModel.RoleName,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Permissions = permissions,
                    };
                    TempData["SuccessMsg"] = "A new role has been created successfully.";
                    _roleManager.Create(role);
                }
                else
                {
                    List<Permission> permissionsToRemoved = new List<Permission>();

                    Roles roleToEdit = _roleManager.Roles.SingleOrDefault(r => r.Name == viewModel.RoleName);
                    roleToEdit.Name = viewModel.RoleName;
                    roleToEdit.UpdatedAt = DateTime.Now;
                    roleToEdit.IsActive = true;
                    _roleManager.Update(roleToEdit);

                    var permissionInRole = _permissionRepository.GetAllInclude(includeProperties: "Roles").ToList().Where(r => r.Roles == roleToEdit);

                    foreach (var fp in viewModel.FeaturePermissions)
                    {
                        foreach (var permission in fp.Permissions ?? new List<PermissionSelectViewModel>())
                        {
                            if (permission != null && !permission.IsSelected)
                            {
                                permissionsToRemoved.Add(_permissionRepository.GetById(permission.PermissionId));
                            }
                        }
                    }

                    foreach (var p in permissionsToRemoved)
                    {
                        p.Roles.Remove(roleToEdit);
                        _permissionRepository.Update(p);
                    }


                    roleToEdit.Permissions = permissions;
                    _roleManager.Update(roleToEdit);
                    _permissionRepository.Dispose();
                    TempData["SuccessMsg"] = "Role '" + roleToEdit.Name + "' has been updated";

                }
                return RedirectToAction("RoleManagement", "System");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditRole(string name)
        {
            var role = _roleStore.Roles.Include(p => p.Permissions).Include(u => u.Users).SingleOrDefault(r => r.Name == name);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features").OrderBy(r => r.features.Name).ToList();
            var features = _featuresRepository.GetAll().Select(r => r.Name);

            EditRoleViewModel viewModel = new EditRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsActive = role.IsActive,
                FeaturePermissions = new List<FeaturePermission>()
            };

            foreach (var feature in features)
            {
                viewModel.FeaturePermissions.Add(new FeaturePermission
                {
                    FeatureName = feature
                });
            }

            foreach (var featurePermission in viewModel.FeaturePermissions)
            {
                featurePermission.Permissions = new List<PermissionSelectViewModel>();

                foreach (var permission in permissions)
                {
                    if (featurePermission.FeatureName == permission.features.Name)
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

            return View("NewRole", viewModel);
        }

    }
}