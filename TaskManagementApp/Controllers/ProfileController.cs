using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;
using TaskManagementApp.App_Start;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]

    public class ProfileController : Controller
    {
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;
        private ApplicationUserManager _appManager;
        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private PermissionRepository _permissionRepository;

        public ProfileController()
        {
            _userStore = new UserStore<ApplicationUser>(new TaskContext());
            _userManager = new UserManager<ApplicationUser>(_userStore);
            _roleStore = new RoleStore<Roles>(new TaskContext());
            _permissionRepository = new PermissionRepository(new TaskContext());

        }

        public ProfileController(ApplicationUserManager appManager)
        {
            ApplicationUserManager = appManager;
        }

        public ApplicationUserManager ApplicationUserManager
        {
            get
            {
                return _appManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _appManager = value;
            }
        }

        // GET: Profile
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var user = _userStore.Users.SingleOrDefault(u => u.Id == currentUser);
            var roleName = _userManager.GetRoles(currentUser)[0];

            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == roleName);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();


            EditProfileViewModel viewModel = new EditProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastLogin = user.LastLogin,
                LastName = user.LastName,
                Username = user.UserName,
                UserRole = roleName,
                UserPermissions = new List<Permission>()
            };

            foreach(var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDetail(EditProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userInDb = _userStore.Users.SingleOrDefault(u => u.Id == viewModel.Id);
                userInDb.FirstName = viewModel.FirstName;
                userInDb.LastName = viewModel.LastName;
                userInDb.Email = viewModel.Email;
                userInDb.UserName = viewModel.Username;
                userInDb.UpdatedAt = DateTime.Now;

                viewModel.UserRole = _userManager.GetRoles(userInDb.Id)[0];
                viewModel.LastLogin = userInDb.LastLogin;

                _userManager.Update(userInDb);
                TempData["SuccessMsg"] = "Your user profile has been updated successfully";
                RedirectToAction("Index", "Profile", viewModel);
            }
            else
            {
                TempData["ErrorMsg"] = "Oops, something went wrong, please go thru error message";
            }

            return View("Index", viewModel);
        }

        public ActionResult EditPassword()
        {
            var currentUser = User.Identity.GetUserId();
            var roleName = _userManager.GetRoles(currentUser)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == roleName);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();


            EditProfilePasswordViewModel viewModel = new EditProfilePasswordViewModel
            {
                Id = User.Identity.GetUserId(),
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPassword(EditProfilePasswordViewModel viewModel)
        {
            var user = _userStore.Users.SingleOrDefault(u => u.Id == viewModel.Id);
            var role = _userManager.GetRoles(viewModel.Id)[0];

            if (ModelState.IsValid)
            {
                var userInDb = _userStore.Users.SingleOrDefault(u => u.Id == viewModel.Id);
                var result = await _userManager.ChangePasswordAsync(userInDb.Id, viewModel.CurrentPassword, viewModel.NewPassword);
                userInDb.UpdatedAt = DateTime.Now;
                if (result.Succeeded)
                {
                    _userManager.Update(userInDb);
                    TempData["SuccessMsg"] = "Your password has been updated successfully";
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    TempData["ErrorMsg"] = "Oops, something went wrong, current password mismatch.";
                }
            }else
            {
                TempData["ErrorMsg"] = "Oops, something went wrong, please go thru the error message."; 
            }

            return View("EditPassword", viewModel);
        }

        public ActionResult EditContact()
        {
            var userId = User.Identity.GetUserId();
            var user = _userStore.Users.SingleOrDefault(u => u.Id == userId);
            var roleName = _userManager.GetRoles(userId)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == roleName);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();

            EditContactViewModel viewModel = new EditContactViewModel
            {
                PhoneNumber = user.PhoneNumber == null ? string.Empty : user.PhoneNumber,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditContact(EditContactViewModel viewModel)
        {
            if(ModelState.IsValid)
            {
                var code = await ApplicationUserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), viewModel.PhoneNumber);

                if(ApplicationUserManager.SmsService != null)
                {
                    var message = new IdentityMessage
                    {
                        Destination = viewModel.PhoneNumber,
                        Body = "Your security code is: " + code
                    };

                    await ApplicationUserManager.SmsService.SendAsync(message);
                    return RedirectToAction("VerifyPhoneNumber", new {PhoneNumber =  viewModel.PhoneNumber});
                }

            }
            TempData["ErrorMsg"] = "Oops, something went wrong, please go thru the error message.";
            return View(viewModel);
        }

        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await ApplicationUserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel {  PhoneNumber = phoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await ApplicationUserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), viewModel.PhoneNumber, viewModel.Code);

                if (result.Succeeded)
                {
                    TempData["SuccessMsg"] = "Your contact has been updated.";
                    return RedirectToAction("Index", "Profile");
                }

            }
            TempData["ErrorMsg"] = "Failed to verify phone";
            return View(viewModel);
        }


    }
}