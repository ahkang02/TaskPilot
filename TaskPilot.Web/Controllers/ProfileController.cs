using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Repository;
using TaskPilot.Web.ViewModels;
using Vonage.Users;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class ProfileController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private UserManager<ApplicationUser> _userManager;
        private ISmsSender _smsSender;

        public ProfileController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ISmsSender smsSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _smsSender = smsSender;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            EditProfileViewModel viewModel = new EditProfileViewModel();
            if (claim != null)
            {
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
                var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

                viewModel = new EditProfileViewModel
                {
                    Email = currentUser.Email,
                    FirstName = currentUser.FirstName,
                    Id = currentUser.Id,
                    LastLogin = currentUser.LastLogin,
                    LastName = currentUser.LastName,
                    Username = currentUser.UserName,
                    UserRole = currentUserRole[0],
                    UserPermissions = new List<Permission>()
                };

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(EditProfileViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {

                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
                var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

                if (ModelState.IsValid)
                {
                    var userInDb = _unitOfWork.Users.Get(u => u.Id == viewModel.Id);
                    userInDb.FirstName = viewModel.FirstName;
                    userInDb.LastName = viewModel.LastName;
                    userInDb.Email = viewModel.Email;
                    userInDb.UserName = viewModel.Username;
                    userInDb.UpdatedAt = DateTime.Now;



                    viewModel.UserRole = currentUserRole[0];
                    viewModel.LastLogin = userInDb.LastLogin;

                    _unitOfWork.Users.Update(userInDb);
                    TempData["SuccessMsg"] = "Your user profile has been updated successfully";
                    RedirectToAction("Index", "Profile", viewModel);
                }
                else
                {
                    TempData["ErrorMsg"] = "Oops, something went wrong, please go thru error message";
                }

                viewModel.UserPermissions = new List<Permission>();
                foreach (var permission in permissions)
                {
                    if (permission.Roles.Any(r => r.Id == roles.Id))
                    {
                        viewModel.UserPermissions.Add(permission);
                    }
                }
            }
            return View("Index", viewModel);
        }

        public async Task<IActionResult> EditPassword()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            EditProfilePasswordViewModel viewModel = new EditProfilePasswordViewModel();

            if (claim != null)
            {
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
                var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

                viewModel = new EditProfilePasswordViewModel
                {
                    Id = currentUser.Id,
                    UserPermissions = new List<Permission>()
                };

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(EditProfilePasswordViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                var userInDB = _unitOfWork.Users.Get(u => u.Id == viewModel.Id);
                var result = await _userManager.ChangePasswordAsync(userInDB, viewModel.CurrentPassword, viewModel.NewPassword);
                userInDB.UpdatedAt = DateTime.Now;

                if (result.Succeeded)
                {
                    _unitOfWork.Users.Update(userInDB);
                    _unitOfWork.Save();
                    TempData["SuccessMsg"] = "Your password has been updated successfully";
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    TempData["ErrorMsg"] = "Oops, something went wrong, current password mismatch.";
                }
            }
            TempData["ErrorMsg"] = "Oops, something went wrong, current password mismatch.";
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");
            viewModel.UserPermissions = new List<Permission>();

            foreach (var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }
            return View("EditPassword", viewModel);
        }

        public async Task<IActionResult> EditContact()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

            EditContactViewModel viewModel = new EditContactViewModel
            {
                PhoneNumber = currentUser.PhoneNumber == null ? string.Empty : currentUser.PhoneNumber,
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
        public async Task<IActionResult> EditContact(EditContactViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

            if(ModelState.IsValid)
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, viewModel.PhoneNumber);

                if(_smsSender != null)
                {
                    await _smsSender.SendSmsAsync(to: viewModel.PhoneNumber, body:"Your security code is " + code, text: "Your security code is " + code);
                    return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = viewModel.PhoneNumber });
                }
            }
            TempData["ErrorMsg"] = "Oops, something went wrong, please go thru the error message.";
            viewModel.UserPermissions = new List<Permission>();
            foreach (var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }
            return View(viewModel);
        }

        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, phoneNumber);
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel viewModel)
        {
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);

                var result = await _userManager.ChangePhoneNumberAsync(currentUser, viewModel.PhoneNumber, viewModel.Code);
                if(result.Succeeded)
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
