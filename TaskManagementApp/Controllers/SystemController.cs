using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;
using TaskManagementApp.App_Start;
using Microsoft.Ajax.Utilities;
using System.Security;
using System.Text.RegularExpressions;
using System.Text;

namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]
    public class SystemController : Controller
    {
        private TaskContext _context;
        private PermissionRepository _permissionRepository;
        private PrioritiesRepository _prioritiesRepository;
        private StatusesRepository _statusesRepository;
        private FeaturesRepository _featuresRepository;

        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;

        private ApplicationUserManager _appUserManager;

        public SystemController(ApplicationUserManager appUserManager)
        {
            AppManager = appUserManager;
        }

        public ApplicationUserManager AppManager
        {
            get
            {
                return _appUserManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _appUserManager = value;
            }
        }

        public SystemController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _prioritiesRepository = new PrioritiesRepository(_context);
            _statusesRepository = new StatusesRepository(_context);
            _featuresRepository = new FeaturesRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
            _roleManager = new RoleManager<Roles>(_roleStore);
        }

        #region AssignRole
        public ActionResult AssignRole(string username)
        {
            var user = _userStore.Users.Include(r => r.Roles).FirstOrDefault(u => u.UserName == username);
            AssignRoleViewModel viewModel = new AssignRoleViewModel
            {
                RoleToSelect = _roleStore.Roles.ToList(),
                Username = user.UserName,
                CurrentUserRole = _userManager.GetRoles(user.Id)[0]
            };

            foreach (var role in user.Roles)
            {
                viewModel.RoleId = role.RoleId;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRole(AssignRoleViewModel model)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.UserName == model.Username);
            var role = _roleStore.Roles.SingleOrDefault(r => r.Id == model.RoleId);
            _userManager.RemoveFromRole(user.Id, model.CurrentUserRole);
            _userManager.AddToRole(user.Id, role.Name);

            user.UpdatedAt = DateTime.Now;
            _userManager.Update(user);

            TempData["SuccessMsg"] = user.UserName + "'s Role Updated From " + model.CurrentUserRole + " To " + role.Name;
            return RedirectToAction("UserManagement", "System");
        }

        #endregion

        #region Users
        public ActionResult UserManagement()
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

        public ActionResult NewUser()
        {
            EditUserViewModel viewModel = new EditUserViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewUser(EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                bool userExistInDb = _userManager.Users.SingleOrDefault(u => u.Email == viewModel.Email) != null ? true : false;
                if (!userExistInDb)
                {
                    if (viewModel.UserId == null)
                    {
                        string genereatedPassword = System.Web.Security.Membership.GeneratePassword(12, 1);
                        char[] specialCharacters = { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };

                        ApplicationUser applicationUser = new ApplicationUser
                        {
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            Email = viewModel.Email,
                            FirstName = viewModel.FirstName,
                            LastName = viewModel.LastName,
                            UserName = new MailAddress(viewModel.Email).User
                        };

                        StringBuilder sanitizedName = new StringBuilder(applicationUser.UserName);

                        foreach(char  specialChar in specialCharacters)
                        {
                            sanitizedName.Replace(specialChar, ' ');
                        }


                        applicationUser.UserName = sanitizedName.ToString().Replace(" ", "");

                        var result = await _userManager.CreateAsync(applicationUser, genereatedPassword);

                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(applicationUser.Id, "Default User");

                            string code = await AppManager.GenerateEmailConfirmationTokenAsync(applicationUser.Id);
                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = applicationUser.Id, code = code }, protocol: Request.Url.Scheme);
                            string body = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Template/AccountCreation.cshtml")))
                            {
                                body = reader.ReadToEnd();
                            }
                            body = body.Replace("{Content}", "Account Creation");
                            body = body.Replace("{ConfirmationLink}", callbackUrl);
                            body = body.Replace("{UserName}", applicationUser.UserName);
                            body = body.Replace("{Password}", genereatedPassword);
                            await AppManager.SendEmailAsync(applicationUser.Id, subject: "Account Creation", body: body);
                            TempData["SuccessMsg"] = "A new user has been created successfully.";
                            return RedirectToAction("UserManagement", "System");
                        }

                    }
                    ModelState.AddModelError("", "User existed, please re-try to create or logging into the existing user.");
                }
                else
                {
                    ApplicationUser userToEdit = _userManager.Users.SingleOrDefault(u => u.Id == viewModel.UserId);
                    userToEdit.UserName = viewModel.UserName;
                    userToEdit.LastName = viewModel.LastName;
                    userToEdit.FirstName = viewModel.FirstName;
                    userToEdit.Email = viewModel.Email;
                    var result = _userManager.ChangePassword(userToEdit.Id, viewModel.CurrentPassword, viewModel.NewPassword);

                    if (result.Succeeded)
                    {
                        _userManager.Update(userToEdit);
                    }

                    TempData["SuccessMsg"] = userToEdit.UserName + "'s has been updated";
                    return RedirectToAction("UserManagement", "System");
                }
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditUser(string username)
        {
            var user = _userStore.Users.SingleOrDefault(u => u.UserName == username);
            EditUserViewModel viewModel = new EditUserViewModel
            {
                UserId = user.Id,
                UserName = username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            return View("NewUser", viewModel);
        }

        #endregion

        #region Roles
        public ActionResult RoleManagement()
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

                foreach(var permission in permissions)
                {
                    if(featurePermission.FeatureName == permission.features.Name)
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
                        IsActive =  true,
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

            foreach(var fp in viewModel.FeaturePermissions)
            {
                foreach(var p in fp.Permissions)
                {
                    foreach(var rp in role.Permissions)
                    {
                        if(rp.Id == p.PermissionId)
                        {
                            p.IsSelected = true;
                        }
                    }
                }
            }

            return View("NewRole", viewModel);
        }

        #endregion

        #region Status
        public ActionResult StatusManagement()
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

        public ActionResult NewStatus()
        {
            EditStatusViewModel viewModel = new EditStatusViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewStatus(EditStatusViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Statuses status = new Statuses
                    {
                        Description = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    _statusesRepository.Insert(status);
                    TempData["SuccessMsg"] = "A new status has been created";
                }
                else
                {
                    Statuses statusToEdit = _statusesRepository.GetByName(viewModel.Name);
                    statusToEdit.Description = viewModel.Name;
                    statusToEdit.UpdatedAt = DateTime.Now;

                    _statusesRepository.Update(statusToEdit);
                    TempData["SuccessMsg"] = statusToEdit.Description + "'s Status has been updated.";
                }
                _statusesRepository.Save();
                _statusesRepository.Dispose();
                return RedirectToAction("StatusManagement", "System");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditStatus(string name)
        {
            var statusInDb = _statusesRepository.GetByName(name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Description,
            };

            return View("NewStatus", viewModel);
        }

        #endregion

        #region Priority

        public ActionResult PriorityManagement()
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

        public ActionResult NewPriority()
        {
            EditPriorityViewModel viewModel = new EditPriorityViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPriority(EditPriorityViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Priorities priority = new Priorities
                    {
                        Description = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    _prioritiesRepository.Insert(priority);
                    TempData["SuccessMsg"] = "A new priority indicator has been created";
                }
                else
                {
                    Priorities priorityToEdit = _prioritiesRepository.GetByName(viewModel.Name);
                    priorityToEdit.Description = viewModel.Name;
                    priorityToEdit.UpdatedAt = DateTime.Now;

                    _prioritiesRepository.Update(priorityToEdit);
                    TempData["SuccessMsg"] = priorityToEdit.Description + "'s priority has been updated";
                }
                _prioritiesRepository.Save();
                _prioritiesRepository.Dispose();
                return RedirectToAction("PriorityManagement", "System");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditPriority(string name)
        {
            var priorityInDb = _prioritiesRepository.GetByName(name);
            EditPriorityViewModel viewModel = new EditPriorityViewModel
            {
                Id = priorityInDb.Id,
                Name = priorityInDb.Description,
            };
            return View("NewPriority", viewModel);
        }


        #endregion

        #region Permission
        public ActionResult PermissionManagement()
        {
            return View();
        }

        public ActionResult NewPermission()
        {
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Features = _featuresRepository.GetAll().ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPermission(EditPermissionViewModel viewModel)
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
                        featuresId = viewModel.FeatureId,
                    };
                    TempData["SuccessMsg"] = "A new permission has been created";
                    _permissionRepository.Insert(permission);
                }
                else
                {
                    Permission permissionToEdit = _permissionRepository.GetByName(viewModel.Name);
                    permissionToEdit.Name = viewModel.Name;
                    permissionToEdit.UpdatedAt = DateTime.Now;

                    _permissionRepository.Update(permissionToEdit);
                    TempData["SuccessMsg"] = permissionToEdit.Name + "'s permission has been updated";

                }
                _permissionRepository.Save();
                _permissionRepository.Dispose();
                return RedirectToAction("PermissionManagement", "System");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditPermission(string name)
        {
            var permissionInDb = _permissionRepository.GetByName(name);
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Id = permissionInDb.Id,
                Name = permissionInDb.Name,
            };
            return View("NewPermission", viewModel);
        }

        #endregion
    }
}