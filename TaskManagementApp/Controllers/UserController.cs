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
using System.Text;



namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]
    public class UserController : Controller
    {
        private readonly TaskContext _context;
        private readonly PermissionRepository _permissionRepository;
        private readonly TaskRepository _taskRepository;
        private readonly NotificationRepository _notifRepository;

        private readonly RoleStore<Roles> _roleStore;
        private readonly UserStore<ApplicationUser> _userStore;
        private readonly UserManager<ApplicationUser> _userManager;

        private ApplicationUserManager _appUserManager;

        public UserController(ApplicationUserManager appUserManager)
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

        public UserController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _taskRepository = new TaskRepository(_context);
            _notifRepository = new NotificationRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
        }

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
            EditUserViewModel viewModel = new EditUserViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                bool userExistInDb = _userManager.Users.SingleOrDefault(u => u.Email == viewModel.Email) != null;
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

                        foreach (char specialChar in specialCharacters)
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
                            return RedirectToAction("Index", "User");
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
                    return RedirectToAction("Index", "User");
                }
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult Update(string username)
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

            return View("New", viewModel);
        }

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
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        public ActionResult Delete(string[] userName)
        {
            if (userName.Count() > 0)
            {
                for (int i = 0; i < userName.Length; i++)
                {
                    var userToDelete = userName[i];
                    var user = _userStore.Users.SingleOrDefault(u => u.UserName == userToDelete);

                    if (user != null)
                    {
                        if (_taskRepository.GetAll().Any(u => u.AssignToId == user.Id))
                        {
                            TempData["ErrorMsg"] = "Oops, something went wrong, the user you trying to delete has task tie to them, delete unsuccessful.";
                            return RedirectToAction("Index", "User");
                        }
                        else
                        {
                            var notifInUser = _notifRepository.GetAll().Where(u => u.UserId == user.Id).ToList();
                            if (notifInUser.Count() > 0)
                            {
                                foreach (var notif in notifInUser)
                                {
                                    _notifRepository.Delete(notif);
                                }
                            }
                            _userManager.Delete(user);
                        }

                    }

                }
            }
            TempData["SuccessMsg"] = userName.Length + " users has been deleted successfully";
            return RedirectToAction("Index", "User");
        }

    }
}