using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using TaskManagementApp.App_Start;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]
    public class RoleController : Controller
    {
        private readonly TaskContext _context;
        private readonly PermissionRepository _permissionRepository;
        private readonly FeaturesRepository _featuresRepository;

        private readonly RoleStore<Roles> _roleStore;
        private readonly RoleManager<Roles> _roleManager;
        private readonly UserStore<ApplicationUser> _userStore;
        private readonly UserManager<ApplicationUser> _userManager;

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

        public ActionResult New()
        {
            EditRoleViewModel viewModel = new EditRoleViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDuplicationRequest]
        public ActionResult New(EditRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Roles roles = new Roles
                    {
                        Name = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _roleManager.Create(roles);
                    TempData["SuccessMsg"] = "A new status has been created";
                }
                else
                {
                    Roles rolesToEdit = _roleStore.Roles.SingleOrDefault(r => r.Id ==  viewModel.Id);
                    rolesToEdit.Name = viewModel.Name;
                    rolesToEdit.UpdatedAt = DateTime.Now;

                    _roleManager.Update(rolesToEdit);
                    TempData["SuccessMsg"] = rolesToEdit.Name + "'s Roles has been updated.";
                }
                return RedirectToAction("Index", "Role");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult Update(string name)
        {
            var statusInDb = _roleStore.Roles.SingleOrDefault(r => r.Name == name);
            EditRoleViewModel viewModel = new EditRoleViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Name,
            };

            return View("New", viewModel);
        }

        public ActionResult Delete(string[] roleName)
        {
            if (roleName.Length > 0)
            {
                for (int i = 0; i < roleName.Length; i++)
                {
                    var role = roleName[i];
                    var roleToDelete = _roleStore.Roles.SingleOrDefault(r => r.Name == role);

                    if (roleToDelete != null)
                    {
                        if (roleToDelete.Users.Count > 0)
                        {
                            TempData["ErrorMsg"] = "There are user exist in the role, you can't delete a role when there's user exist.";
                            return RedirectToAction("Index", "Role");
                        }
                        else
                        {
                            _roleManager.Delete(roleToDelete);
                        }
                    }

                }
            }
            TempData["SuccessMsg"] = roleName.Length + " roles deleted successfully";
            return RedirectToAction("Index", "Role");
        }

        public ActionResult AssignPermission(string name)
        {
            var role = _roleStore.Roles.Include(p => p.Permissions).Include(u => u.Users).SingleOrDefault(r => r.Name == name);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features").OrderBy(r => r.Features.Name).ToList();
            var features = _featuresRepository.GetAll().Select(r => r.Name);

            AssignPermissionViewModel viewModel = new AssignPermissionViewModel
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
        [PreventDuplicationRequest]
        public ActionResult AssignPermission(AssignPermissionViewModel viewModel)
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


                List<Permission> permissionsToRemoved = new List<Permission>();

                Roles roleToEdit = _roleManager.Roles.SingleOrDefault(r => r.Id == viewModel.RoleId);
                roleToEdit.UpdatedAt = DateTime.Now;
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
                TempData["SuccessMsg"] = "Role '" + roleToEdit.Name + "' permission has been updated";
                return RedirectToAction("Index", "Role");

            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }
    }
}