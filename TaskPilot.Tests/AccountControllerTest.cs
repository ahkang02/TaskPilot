using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class AccountControllerTest
    {
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<IUrlHelper> _urlHelper;
        private AccountController _accountController;

        [SetUp]
        public void Setup()
        {
            _emailSenderMock = new Mock<IEmailSender>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _accountController = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _emailSenderMock.Object);

            _urlHelper = new Mock<IUrlHelper>();
            _accountController.Url = _urlHelper.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _accountController.Dispose();
        }

        [Test]
        public void CheckSession_UserNotAuthenticated_ReturnsJsonWithExpired()
        {
            // Arrange
            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = _accountController.CheckSession() as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.That(JsonSerializer.Serialize(result.Value), Is.EqualTo("{\"isExpired\":true}"));
        }

        [Test]
        public void CheckSession_UserAuthenticated_ReturnsJsonWithNotExpired()
        {
            // Arrange
            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();
            _accountController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }, "mock"));

            // Act
            var result = _accountController.CheckSession() as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.That(JsonSerializer.Serialize(result.Value), Is.EqualTo("{\"isExpired\":false}"));
        }

        [Test]
        public void Login_WhenUserIsAuthenticated_RedirectsToDashboard()
        {
            // Arrange
            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();



            _accountController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }, "mock"));

            // Act
            var result = _accountController.Login() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Dashboard"));
        }

        [Test]
        public void Login_WhenUserIsNotAuthenticated_RedirectsToLoginView()
        {
            // Mock the HttpContext and UrlHelper
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Content("~/")).Returns("/");

            // Set up the ControllerContext with the mocked HttpContext and UrlHelper


            // Arrange
            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();
            _accountController.Url = urlHelperMock.Object;

            // Act
            var result = _accountController.Login();

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public async Task Login_ValidCredentials_RedirectsToDashboard()
        {
            // Arrange
            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();

            var viewModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "password",
                RememberMe = false
            };

            var user = new ApplicationUser { UserName = viewModel.Username, FirstName = "Test User", LastName = "2" };
            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, viewModel.Password)).ReturnsAsync(true);
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(viewModel.Username, viewModel.Password, viewModel.RememberMe, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _accountController.Login(viewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Dashboard"));
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "nonexistentUser",
                Password = "password"
            };

            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _accountController.Login(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.ContainsKey(""));
            Assert.That(result.ViewData.ModelState[""]!.Errors[0].ErrorMessage, Is.EqualTo(Message.NO_USER_FOUND));
        }

        [Test]
        public async Task Login_InvalidPassword_AddsModelError()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "validUser",
                Password = "invalidPassword"
            };

            var user = new ApplicationUser { UserName = viewModel.Username, FirstName = "Test User", LastName = "2" };
            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, viewModel.Password)).ReturnsAsync(false);
            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _accountController.Login(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.ContainsKey(""));
            Assert.That(result.ViewData.ModelState[""]!.Errors[0].ErrorMessage, Is.EqualTo(Message.INVALID_LOGIN_ATTEMPT));
        }

        [Test]
        public async Task Login_UserLockedOut_ReturnsErrorView()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "lockedOutUser",
                Password = "password"
            };

            var user = new ApplicationUser { UserName = viewModel.Username, FirstName = "Test User", LastName = "2" };
            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, viewModel.Password)).ReturnsAsync(false);
            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.IsLockedOutAsync(user)).ReturnsAsync(true);

            // Act
            var result = await _accountController.Login(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Error"));
            Assert.That(result.ViewData["Message"], Is.EqualTo("You've entered too many times of invalid credentials, please try again after 5 minutes."));
        }

        [Test]
        public void Register_ReturnsView()
        {
            // Act
            var result = _accountController.Register();

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public async Task RegisterExistedUser_ReturnsViewWithModelError()
        {
            var viewModel = new RegisterViewModel
            {
                ConfirmPassword = "password",
                Email = "test@gmail.com",
                FirstName = "Test",
                LastName = "User",
                Password = "password",
                Username = "testuser"
            };


            var user = new ApplicationUser { UserName = viewModel.Username, FirstName = viewModel.FirstName, LastName = viewModel.LastName };
            _userManagerMock.Setup(u => u.Users).Returns((new List<ApplicationUser> { user }).AsQueryable());
            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.FindByEmailAsync(viewModel.Email)).ReturnsAsync(user);


            // Act
            var result = await _accountController.Register(viewModel) as ViewResult;


            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.ContainsKey(""));
            Assert.That(result.ViewData.ModelState[""]!.Errors[0].ErrorMessage, Is.EqualTo(Message.USER_EXIST));
        }

        [Test]
        public async Task RegisterNewUser_ReturnsSuccess()
        {
            var viewModel = new RegisterViewModel
            {
                ConfirmPassword = "Abcd1234!!",
                Email = "test@gmail.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Abcd1234!!",
                Username = "testuser1234"
            };

            string expectedCallbackUrl = "http://localhost/Account/ConfirmEmail";
            var user = new ApplicationUser { UserName = viewModel.Username, FirstName = viewModel.FirstName, LastName = viewModel.LastName };
            string bodyTemplate = "Account Confirmation: {UserName}, {ConfirmationLink}";

            _userManagerMock.Setup(u => u.Users).Returns(Enumerable.Empty<ApplicationUser>().AsQueryable());
            _userManagerMock.Setup(u => u.FindByNameAsync(viewModel.Username)).ReturnsAsync(null as ApplicationUser);
            _userManagerMock.Setup(u => u.FindByEmailAsync(viewModel.Email)).ReturnsAsync(null as ApplicationUser);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), viewModel.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), SD.DEFAULT_ROLE)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("dummyToken");
            _urlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(expectedCallbackUrl);
            _emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Scheme).Returns("http");
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var streamReaderMock = new MockStreamReader(bodyTemplate);
            _accountController.StreamReaderFactory = path => streamReaderMock;

            // Act
            var result = await _accountController.Register(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task ConfirmEmail_NullValueReturnUnauthorized()
        {
            // Arrange

            // Act
            var result = await _accountController.ConfirmEmail(null, null) as UnauthorizedResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ConfirmEmail_UserNotFound()
        {

            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _accountController.ConfirmEmail("dummyUserId", "dummyToken") as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task ConfirmEmail_AlreadyConfirmed()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser1234", FirstName = "Test User", LastName = "2", EmailConfirmed = true };
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act
            var result = await _accountController.ConfirmEmail("dummyUserId", "dummyToken") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("AlreadyConfirmed"));
            Assert.That(result.ViewData["Message"], Is.EqualTo("Your email is already confirmed."));
        }

        [Test]
        public async Task ConfirmEmail_Valid()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser1234", FirstName = "Test User", LastName = "2", EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ConfirmEmailAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.ConfirmEmail("dummyUserId", "dummyToken") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("ConfirmEmail"));
        }

        [Test]
        public async Task ConfirmEmail_ExpiredToken()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser1234", FirstName = "Test User", LastName = "2", EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ConfirmEmailAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken" }));

            // Act
            var result = await _accountController.ConfirmEmail("dummyUserId", "dummyToken") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Error"));
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("The confirmation link has expired. Please request a new confirmation email."));
        }

        [Test]
        public void AlreadyConfirmed_ReturnView()
        {
            // Act
            var result = _accountController.AlreadyConfirmed();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResendConfirmationEmail_ReturnView()
        {
            // Act
            var result = _accountController.ResendConfirmation();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task ResendConfirmationEmail_Valid()
        {
            //Arrange
            var viewModel = new ResendConfirmationViewModel
            {
                Email = "abcd1234@gmail.com"
            };

            var user = new ApplicationUser { UserName = "testuser1234", FirstName = "Test User", LastName = "2", Email = viewModel.Email, EmailConfirmed = false };
            var callbackUrl = "http://localhost/Account/ConfirmEmail";
            string bodyTemplate = "Account Confirmation: {UserName}, {ConfirmationLink}";

            _userManagerMock.Setup(u => u.Users).Returns(Enumerable.Empty<ApplicationUser>().AsQueryable());
            _userManagerMock.Setup(u => u.FindByEmailAsync(viewModel.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("dummyToken");
            _urlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(callbackUrl);
            _emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Scheme).Returns("http");
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var streamReaderMock = new MockStreamReader(bodyTemplate);
            _accountController.StreamReaderFactory = path => streamReaderMock;

            // Act
            var result = await _accountController.ResendConfirmation(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Login"));
        }

        [Test]
        public void ForgotPassword_ReturnView()
        {
            // Act
            var result = _accountController.ForgotPassword();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ForgotPasswordConfirmation_ReturnView()
        {
            // Act
            var result = _accountController.ForgotPasswordConfirmation();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task ResetPasswordWithEmptyCode_ReturnUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _accountController.ResetPassword(null, null) as UnauthorizedResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ResetPasswordWithInvalidToken_ReturnError()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
            _userManagerMock.Setup(u => u.VerifyUserTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _accountController.ResetPassword(null, "dummyToken") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Error"));
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("The reset password link has expired. Please request a new reset password email."));
        }

        [Test]
        public async Task ResetPasswordValidToken_ReturnView()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
            _userManagerMock.Setup(u => u.VerifyUserTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _accountController.ResetPassword(null, "dummyToken") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task ResetPsswordWithInvalidUser_ReturnRedirectToConfirmation()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Code = "dummyToken",
                ConfirmPassword = "Abcd1234!!",
                Email = "test@gmail.com",
                Password = "Abcd1234!!"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(viewModel.Email)).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _accountController.ResetPassword(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("ResetPasswordConfirmation"));
        }

        [Test]
        public async Task ResetPasswordWithValidUserValidToken_ReturnRedirectToConfirmation()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Code = "dummyToken",
                ConfirmPassword = "Abcd1234!!",
                Email = "test@gmail.com",
                Password = "Abcd1234!!"
            };

            var user = new ApplicationUser { UserName = "testuser1234", FirstName = "Test User", LastName = "2", Email = viewModel.Email };
            _userManagerMock.Setup(u => u.FindByEmailAsync(viewModel.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.ResetPassword(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("ResetPasswordConfirmation"));
        }

        [Test]
        public void ResetPasswordConfirmation_ReturnView()
        {
            // Act
            var result = _accountController.ResetPasswordConfirmation();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task LogOff_LogUserOff_ReturnRedirectToLogin()
        {
            // Arrange
            _signInManagerMock.Setup(u => u.SignOutAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _accountController.LogOff() as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Login"));
        }

    }
    public class MockStreamReader : StreamReader
    {
        private readonly string _content;

        public MockStreamReader(string content) : base(new MemoryStream())
        {
            _content = content;
        }

        public override Task<string> ReadToEndAsync()
        {
            return Task.FromResult(_content);
        }
    }
}
