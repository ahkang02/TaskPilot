using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
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
    public class UserController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IUserPermissionService _userPermissionService;
        private readonly ITaskService _taskService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, RoleManager<ApplicationRole> roleManager, IUserPermissionService userPermissionService, ITaskService taskService, INotificationService notificationService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _userPermissionService = userPermissionService;
            _taskService = taskService;
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = _userPermissionService.GetUserPermission(claimsIdentity).ToList()
            };
            return View(viewModel);
        }

        public IActionResult New()
        {
            EditUserViewModel viewModel = new EditUserViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                bool userExistInDB = await _userManager.FindByEmailAsync(viewModel.Email!) != null;

                if (!userExistInDB)
                {
                    string generatedPassword = GeneratePassword.Generate(12, 1);
                    char[] specialCharacters = { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };

                    ApplicationUser applicationUser = new ApplicationUser
                    {
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Email = viewModel.Email,
                        FirstName = viewModel.FirstName!,
                        LastName = viewModel.LastName!,
                        UserName = new MailAddress(viewModel.Email!).User
                    };

                    StringBuilder sanitizedName = new StringBuilder(applicationUser.UserName);

                    foreach (char specialChar in specialCharacters)
                    {
                        sanitizedName.Replace(specialChar, ' ');
                    }

                    applicationUser.UserName = sanitizedName.ToString().Replace(" ", "");

                    var result = await _userManager.CreateAsync(applicationUser, generatedPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(applicationUser, SD.DEFAULT_ROLE);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = applicationUser.Id, code = code }, protocol: Request.Scheme);
                        string body = string.Empty;

                        using (StreamReader reader = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "AccountCreation.html")))
                        {
                            body = await reader.ReadToEndAsync();
                        }

                        body = body.Replace("{Content}", "Account Creation");
                        body = body.Replace("{ConfirmationLink}", callbackUrl);
                        body = body.Replace("{UserName}", applicationUser.UserName);
                        body = body.Replace("{Password}", generatedPassword);
                        await _emailSender.SendEmailAsync(applicationUser.Email!, subject: "Account Creation", htmlMessage: body);
                        TempData["SuccessMsg"] = Message.USER_CREATION;

                        return RedirectToAction("Index", "User");
                    }
                }
                ModelState.AddModelError("", Message.USER_CREATION_FAIL);
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Update(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            EditUserViewModel viewModel = new EditUserViewModel
            {
                UserId = user!.Id,
                UserName = username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            return View("New", viewModel);
        }

        public async Task<IActionResult> Delete(string[] userName)
        {
            var userToDelete = new List<ApplicationUser>();
            if (userName.Length > 0)
            {
                for (int i = 0; i < userName.Length; i++)
                {
                    var uName = userName[i];
                    userToDelete.Add(await _userManager.FindByNameAsync(uName!));
                }

                foreach (var user in userToDelete)
                {
                    if (_taskService.IsUserHoldingTask(user.Id))
                    {
                        TempData["ErrorMsg"] = Message.USER_DELETION_FAIL;
                        return BadRequest(new { data = Url.Action("Index", "User") });
                    }
                    else
                    {
                        var notifInUser = _notificationService.GetNotificationByUserId(user.Id);
                        if (notifInUser.Any())
                        {
                            _notificationService.DeleteAllNotification(notifInUser);
                        }
                        await _userManager.DeleteAsync(user);
                    }
                }
            }
            TempData["SuccessMsg"] = userName.Length + Message.USER_DELETION;
            return Json(Url.Action("Index", "User"));
        }

        public async Task<ActionResult> AssignRole(string username)
        {
            var user = await _userManager.Users.Include("UserRoles").FirstOrDefaultAsync(u => u.UserName == username);
            var userCurrentRole = await _userManager.GetRolesAsync(user!);

            AssignRoleViewModel viewModel = new AssignRoleViewModel
            {
                RoleToSelect = await _roleManager.Roles.ToListAsync(),
                Username = user!.UserName,
                CurrentUserRole = userCurrentRole[0]
            };

            foreach (var role in user.UserRoles!)
            {
                viewModel.RoleId = role.RoleId;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(viewModel.Username!);
                var role = await _roleManager.FindByIdAsync(viewModel.RoleId!);

                await _userManager.RemoveFromRoleAsync(user!, viewModel.CurrentUserRole!);
                await _userManager.AddToRoleAsync(user!, role!.Name!);

                user!.UpdatedAt = DateTime.Now;
                await _userManager.UpdateAsync(user);

                TempData["SuccessMsg"] = user.UserName + "'s Role Updated From " + viewModel.CurrentUserRole + " To " + role.Name;
            }
            return RedirectToAction("Index", "User");
        }

    }
}
