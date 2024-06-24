using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class ProfileController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;

        public ProfileController(UserManager<ApplicationUser> userManager, ISmsSender smsSender, IEmailSender emailSender, RoleManager<ApplicationRole> roleManager, IPermissionService permissionService)
        {
            _userManager = userManager;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        {
            var username = User.Identity!.Name;
            return await _userManager.FindByNameAsync(username!) ?? null;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);
            var permissions = _permissionService.GetPermissionInRole(roles!);

            EditProfileViewModel viewModel = new EditProfileViewModel
            {
                Email = currentUser!.Email!,
                FirstName = currentUser.FirstName,
                Id = currentUser.Id,
                LastLogin = currentUser.LastLogin,
                LastName = currentUser.LastName,
                Username = currentUser.UserName!,
                UserRole = currentUserRole[0],
                UserPermissions = permissions.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(EditProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userInDb = await _userManager.FindByIdAsync(viewModel.Id);

                if (userInDb!.UserName != viewModel.Username)
                {
                    bool isUserNameExist = await _userManager.FindByNameAsync(viewModel.Username!) != null;
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
                            body = await reader.ReadToEndAsync();
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
                    await _userManager.UpdateAsync(userInDb);
                    TempData["SuccessMsg"] = Message.PROF_DETAIL_EDIT;
                }

            }
            else
            {
                TempData["ErrorMsg"] = Message.COMMON_ERROR;
            }

            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);


            viewModel.UserPermissions = _permissionService.GetPermissionInRole(roles!).ToList();
            viewModel.UserRole = currentUserRole[0];
            viewModel.LastLogin = currentUser!.LastLogin;

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
            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);
            var permissions = _permissionService.GetPermissionInRole(roles!);

            EditProfilePasswordViewModel viewModel = new EditProfilePasswordViewModel
            {
                Id = currentUser!.Id,
                UserPermissions = permissions.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(EditProfilePasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userInDB = await _userManager.FindByIdAsync(viewModel.Id);
                var result = await _userManager.ChangePasswordAsync(userInDB, viewModel.CurrentPassword!, viewModel.NewPassword!);

                if (result.Succeeded)
                {
                    userInDB.UpdatedAt = DateTime.Now;
                    await _userManager.UpdateAsync(userInDB);
                    TempData["SuccessMsg"] = Message.PROF_PASS_EDIT;
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    TempData["ErrorMsg"] = Message.PROF_PASS_EDIT_FAIL;
                }
            }

            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);
            viewModel.UserPermissions = _permissionService.GetPermissionInRole(roles!).ToList();

            return View("EditPassword", viewModel);
        }

        public async Task<IActionResult> EditContact()
        {
            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);
            var permissions = _permissionService.GetPermissionInRole(roles!);

            EditContactViewModel viewModel = new EditContactViewModel
            {
                PhoneNumber = currentUser!.PhoneNumber == null ? string.Empty : currentUser.PhoneNumber,
                UserPermissions = permissions!.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContact(EditContactViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUser(), viewModel.PhoneNumber);

                if (_smsSender != null)
                {
                    await _smsSender.SendSmsAsync(to: viewModel.PhoneNumber, body: "Your security code is " + code, text: "Your security code is " + code);
                    return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = viewModel.PhoneNumber });
                }
            }

            TempData["ErrorMsg"] = Message.COMMON_ERROR;

            var currentUser = await GetCurrentUser();
            var currentUserRole = await _userManager.GetRolesAsync(currentUser!);
            var roles = await _roleManager.Roles.Include("Permissions").FirstOrDefaultAsync(r => r.Name == currentUserRole[0]);
            viewModel.UserPermissions = _permissionService.GetPermissionInRole(roles!).ToList();

            return View(viewModel);
        }

        public IActionResult VerifyPhoneNumber(string phoneNumber)
        {
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.ChangePhoneNumberAsync(await GetCurrentUser(), viewModel.PhoneNumber!, viewModel.Code!);
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
