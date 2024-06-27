using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            bool isExpired = !User!.Identity!.IsAuthenticated;
            return Json(new { isExpired });
        }

        public IActionResult Login(string? returnUrl = null)
        {
            if (User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
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
                        ViewBag.EmailConfirmation = false;
                        return View(viewModel);
                    }

                    var validPassword = _userManager.CheckPasswordAsync(user, viewModel.Password!);
                    if (!validPassword.Result)
                    {
                        ModelState.AddModelError("", Message.INVALID_LOGIN_ATTEMPT);
                        return View(viewModel);
                    }
                    else
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
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
                bool isUserExisted = _userManager.Users.FirstOrDefault(u => u.UserName == viewModel.Username || u.Email == viewModel.Email) != null;

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
                }
                ModelState.AddModelError("", Message.USER_EXIST);
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

            if(user == null)
            {
                return NotFound("User not found");
            }

            if(user.EmailConfirmed)
            {
                ViewBag.Message = "Your email is already confirmed.";
                return View("AlreadyConfirmed");
            }

            var result = await _userManager.ConfirmEmailAsync(user!, code);

            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return View("ConfirmEmail");
            }
            else if (result.Errors.Any(e => e.Code == "InvalidToken"))
            {
                ViewBag.ErrorMessage = "The confirmation link has expired. Please request a new confirmation email.";
                return View("Error");
            }

            return View(result.Succeeded ? "ConfirmEmail" : BadRequest());
        }

        [AllowAnonymous]
        public IActionResult AlreadyConfirmed()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResendConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email!);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);
                string body = string.Empty;

                using (StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "AccountConfirmation.html")))
                {
                    body = await reader.ReadToEndAsync();
                }

                body = body.Replace("{Content}", "Account Confirmation");
                body = body.Replace("{ConfirmationLink}", callbackUrl);
                body = body.Replace("{UserName}", user.UserName);
                await _emailSender.SendEmailAsync(user.Email!, subject: "Confirm your account", htmlMessage: body);

                return View("Login");
            }
            return View(viewModel);
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

        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            var  user = await _userManager.FindByIdAsync(userId);

            if (code == null)
            {
                return Unauthorized();
            }

            var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)));
            if (!isValidToken)
            {
                ViewBag.ErrorMessage = "The reset password link has expired. Please request a new reset password email.";
                return View("Error");
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
                await _userManager.UpdateSecurityStampAsync(user);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            else if (result.Errors.Any(e => e.Code == "InvalidToken"))
            {
                ViewBag.ErrorMessage = "The reset password link has expired. Please request a new reset password email.";
                return View("Error");
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
