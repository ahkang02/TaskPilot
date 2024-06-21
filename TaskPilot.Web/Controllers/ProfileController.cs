using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;

        public ProfileController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ISmsSender smsSender, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

            EditProfileViewModel viewModel = new EditProfileViewModel
            {
                Email = currentUser.Email!,
                FirstName = currentUser.FirstName,
                Id = currentUser.Id,
                LastLogin = currentUser.LastLogin,
                LastName = currentUser.LastName,
                Username = currentUser.UserName!,
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
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(EditProfileViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
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

                    if (userInDb.UserName != viewModel.Username)
                    {
                        bool isUserNameExist = _unitOfWork.Users.Get(u => u.UserName == viewModel.Username) != null;
                        if (isUserNameExist)
                        {
                            TempData["ErrorMsg"] = Message.PROF_USERNAME_EXIST;
                            viewModel.Username = userInDb.UserName;
                        }
                    }
                    else
                    {

                        if (userInDb.Email != viewModel.Email)
                        {
                            var code = await _userManager.GenerateChangeEmailTokenAsync(userInDb, viewModel.Email!);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Action("ChangeEmail", "Profile", new { userId = userInDb.Id, viewModel.Email, code }, protocol: Request.Scheme);
                            string body = string.Empty;

                            using (StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "AccountConfirmation.html")))
                            {
                                body = reader.ReadToEnd();
                            }

                            body = body.Replace("{Content}", "Email Change Request");
                            body = body.Replace("{ConfirmationLink}", callbackUrl);
                            body = body.Replace("{UserName}", userInDb.UserName);
                            await _emailSender.SendEmailAsync(viewModel.Email!, subject: "Confirm your email change request", htmlMessage: body);

                            viewModel.Email = userInDb.Email;
                        }

                        userInDb.FirstName = viewModel.FirstName!;
                        userInDb.LastName = viewModel.LastName!;
                        userInDb.UserName = viewModel.Username;
                        userInDb.UpdatedAt = DateTime.Now;
                        _unitOfWork.Users.Update(userInDb);
                        TempData["SuccessMsg"] = Message.PROF_DETAIL_EDIT;
                    }
                    viewModel.UserRole = currentUserRole[0];
                    viewModel.LastLogin = userInDb.LastLogin;
                }
                else
                {
                    TempData["ErrorMsg"] = Message.COMMON_ERROR;
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

        [AllowAnonymous]
        public async Task<IActionResult> ChangeEmail(string userId, string email, string code)
        {
            if (userId == null || code == null || email == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);

            if (result.Succeeded)
            {
                TempData["SuccessMsg"] = Message.PROF_EMAIL_EDIT;
                return RedirectToAction("Index", "Profile");
            }

            TempData["ErrorMsg"] = Message.PROF_EMAIL_EDIT_FAIL;
            return RedirectToAction("Index", "Profile");
        }

        public async Task<IActionResult> EditPassword()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

            EditProfilePasswordViewModel viewModel = new EditProfilePasswordViewModel
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


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(EditProfilePasswordViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                var userInDB = _unitOfWork.Users.Get(u => u.Id == viewModel.Id);
                var result = await _userManager.ChangePasswordAsync(userInDB, viewModel.CurrentPassword!, viewModel.NewPassword!);
                userInDB.UpdatedAt = DateTime.Now;

                if (result.Succeeded)
                {
                    _unitOfWork.Users.Update(userInDB);
                    _unitOfWork.Save();
                    TempData["SuccessMsg"] = Message.PROF_PASS_EDIT;
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    TempData["ErrorMsg"] = Message.PROF_PASS_EDIT_FAIL;
                }
            }
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
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
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
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
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
            var currentUserRole = await _userManager.GetRolesAsync(currentUser);
            var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
            var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

            if (ModelState.IsValid)
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, viewModel.PhoneNumber);

                if (_smsSender != null)
                {
                    await _smsSender.SendSmsAsync(to: viewModel.PhoneNumber, body: "Your security code is " + code, text: "Your security code is " + code);
                    return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = viewModel.PhoneNumber });
                }
            }

            TempData["ErrorMsg"] = Message.COMMON_ERROR;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, phoneNumber);
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity!;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);

                var result = await _userManager.ChangePhoneNumberAsync(currentUser, viewModel.PhoneNumber!, viewModel.Code!);
                if (result.Succeeded)
                {
                    TempData["SuccessMsg"] = Message.PROF_CONTACT_EDIT;
                    return RedirectToAction("Index", "Profile");
                }
            }
            TempData["ErrorMsg"] = Message.PROF_CONTACT_EDIT_FAIL;
            return View(viewModel);
        }

    }
}
