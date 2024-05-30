using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Login(string? returnUrl = null)
        {
            if (User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            returnUrl ??= Url.Content("~/");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            else
            {
                var user = await _userManager.FindByNameAsync(viewModel.Username!);
                if (user != null)
                {

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ViewBag.Title = "Error: Email Confirmation Missing";
                        ViewBag.ErrorMessage = Message.EMAIL_CONFIRMATION;
                        return View("Error");
                    }
                    var result = await _signInManager.PasswordSignInAsync(viewModel.Username!, viewModel.Password!, viewModel.RememberMe, true);

                    if (result.Succeeded)
                    {
                        user.LastLogin = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        if (string.IsNullOrEmpty(viewModel.returnUrl))
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            return LocalRedirect(viewModel.returnUrl);
                        }
                    }
                    else if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError("", Message.INVALID_LOGIN_ATTEMPT);
                        return View(viewModel);
                    }
                }
                else
                {
                    ModelState.AddModelError("", Message.NO_USER_FOUND);
                    return View(viewModel);
                }

            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                bool isUserExisted = _unitOfWork.Users.Get(u => u.UserName == viewModel.Username || u.Email == viewModel.Email) != null;

                if (!isUserExisted)
                {
                    var newUser = new ApplicationUser { UserName = viewModel.Username, Email = viewModel.Email, FirstName = viewModel.FirstName!, LastName = viewModel.LastName!, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                    var result = await _userManager.CreateAsync(newUser, viewModel.Password!);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.DEFAULT_ROLE);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, code }, protocol: Request.Scheme);
                        string body = string.Empty;

                        using (StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "AccountConfirmation.html")))
                        {
                            body = reader.ReadToEnd();
                        }

                        body = body.Replace("{Content}", "Account Confirmation");
                        body = body.Replace("{ConfirmationLink}", callbackUrl);
                        body = body.Replace("{UserName}", newUser.UserName);
                        await _emailSender.SendEmailAsync(newUser.Email!, subject: "Confirm your account", htmlMessage: body);

                        return View("Login");
                    }
                    ModelState.AddModelError("", Message.USER_EXIST);
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Unauthorized();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ConfirmEmailAsync(user!, code);
            return View(result.Succeeded ? "ConfirmEmail" : BadRequest());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email!)!;

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return View("ForgotPasswordConfirmation");
                }
                else
                {

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);
                    string body = string.Empty;

                    using (StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "AccountConfirmation.html")))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{Content}", "Reset Password");
                    body = body.Replace("{ConfirmationLink}", callbackUrl);
                    body = body.Replace("{UserName}", user.UserName);
                    await _emailSender.SendEmailAsync(user.Email!, subject: "Reset Password", htmlMessage: body);

                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]

        public IActionResult ResetPassword(string code)
        {
            if (code == null)
            {
                return Unauthorized();
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _userManager.FindByEmailAsync(viewModel.Email!);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            viewModel.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(viewModel.Code!));
            var result = await _userManager.ResetPasswordAsync(user, viewModel.Code, viewModel.Password!);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            ModelState.AddModelError("", result.ToString());
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
