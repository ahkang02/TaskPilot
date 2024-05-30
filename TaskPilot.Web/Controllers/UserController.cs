using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Repository;
using TaskPilot.Web.ViewModels;
using Vonage.Users;
                                                                                                                                                                                                                                                                                                                        
namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            return View(Helper.GetUserPermission(_unitOfWork, claimsIdentity));
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
                bool userExistInDB = _unitOfWork.Users.Get(u => u.Email == viewModel.Email) != null;

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
                            body = reader.ReadToEnd();
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

        public IActionResult Update(string username)
        {
            var user = _unitOfWork.Users.Get(u => u.UserName == username);
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

        public IActionResult Delete(string[] userName)
        {
            var userToDelete = new List<ApplicationUser>();
            if (userName.Count() > 0)
            {
                for (int i = 0; i < userName.Length; i++)
                {
                    var uName = userName[i];
                    userToDelete.Add(_unitOfWork.Users.Get(u => u.UserName == uName));
                }

                foreach (var user in userToDelete)
                {
                    if (_unitOfWork.Tasks.GetAll().Any(u => u.AssignToId == user.Id))
                    {
                        TempData["ErrorMsg"] = Message.USER_DELETION_FAIL;
                        return BadRequest(new { data = Url.Action("Index", "User") });
                    }
                    else
                    {
                        var notifInUser = _unitOfWork.Notification.GetAll().Where(u => u.UserId == user.Id).ToList();
                        if (notifInUser.Count() > 0)
                        {
                            _unitOfWork.Notification.RemoveRange(notifInUser);
                        }
                        _unitOfWork.Users.Remove(user);
                    }
                }
            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = userName.Length + Message.USER_DELETION;
            return Json(Url.Action("Index", "User"));
        }

        public async Task<ActionResult> AssignRole(string username)
        {
            var user = _unitOfWork.Users.GetAllInclude(u => u.UserName == username, "UserRoles").First();
            var userCurrentRole = await _userManager.GetRolesAsync(user);

            AssignRoleViewModel viewModel = new AssignRoleViewModel
            {
                RoleToSelect = _unitOfWork.Roles.GetAll().ToList(),
                Username = user.UserName,
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
            var user = _unitOfWork.Users.Get(u => u.UserName == viewModel.Username);
            var role = _unitOfWork.Roles.Get(r => r.Id == viewModel.RoleId);

            await _userManager.RemoveFromRoleAsync(user, viewModel.CurrentUserRole!);
            await _userManager.AddToRoleAsync(user, role.Name!);

            user.UpdatedAt = DateTime.Now;
            _unitOfWork.Users.Update(user);

            _unitOfWork.Save();

            TempData["SuccessMsg"] = user.UserName + "'s Role Updated From " + viewModel.CurrentUserRole + " To " + role.Name;
            return RedirectToAction("Index", "User");
        }

    }
}
