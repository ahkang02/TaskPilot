using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.APIs;
using TaskPilot.Web.Controllers;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class ProfileControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private Mock<ISmsSender> _mockSmsSender;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<IPermissionService> _mockPermissionService;
        private ProfileController _controller;
        private ApplicationUser _mockUser;
        private ApplicationRole _mockRole;
        private List<Permission> _mockPermission;
        private Mock<IUrlHelper> _urlHelper;


        [SetUp]
        public void SetUp()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
                Mock.Of<IRoleStore<ApplicationRole>>(), null, null, null, null);

            _mockSmsSender = new Mock<ISmsSender>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockPermissionService = new Mock<IPermissionService>();
            _urlHelper = new Mock<IUrlHelper>();

            _controller = new ProfileController(
                _mockUserManager.Object,
                _mockSmsSender.Object,
                _mockEmailSender.Object,
                _mockRoleManager.Object,
                _mockPermissionService.Object);

            // Mock Data
            _mockUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Id = "1",
                LastLogin = DateTime.Now
            };

            _mockRole = new ApplicationRole { Name = "TestRole" };

            var testFeature = new Features { Name = "TestFeature", Id = Guid.NewGuid() };

            _mockPermission = new List<Permission> { new Permission { Features = testFeature, FeaturesId = testFeature.Id, Name = "TestPermission" } };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };


            _controller.TempData = new Mock<ITempDataDictionary>().Object;
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
            _controller.Url = _urlHelper.Object;

            _mockUserManager.Setup(um => um.FindByNameAsync("testuser")).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(um => um.GetRolesAsync(_mockUser)).ReturnsAsync(new List<string> { "TestRole" });
            var roles = new List<ApplicationRole> { _mockRole }.AsQueryable().BuildMock();
            _mockRoleManager.Setup(rm => rm.Roles).Returns(roles);
            _mockPermissionService.Setup(ps => ps.GetPermissionInRole(_mockRole)).Returns(_mockPermission);

        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithExpectedViewModel()
        {
            // Act
            var result = await _controller.Index() as ViewResult;
            var viewModel = result?.Model as EditProfileViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsNotNull(viewModel);
            Assert.That(viewModel.Email, Is.EqualTo("testuser@example.com"));
            Assert.That(viewModel.FirstName, Is.EqualTo("Test"));
        }

        [Test]
        public async Task EditDetail_InvalidModelState_ReturnViews()
        {
            // Arrange
            var viewModel = new EditProfileViewModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "testuser@example.com",
                Id = "Test",
                UserRole = "TestRole"
            };

            _controller.ModelState.AddModelError("FirstName", "Required");

            // Act
            var result = await _controller.EditDetail(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.TempData["ErrorMsg"], Is.EqualTo(Message.COMMON_ERROR));
            Assert.That(result.ViewName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task EditDetail_ValidModelState_DuplicateUsernameFound()
        {
            // Arrange
            var viewModel = new EditProfileViewModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "testuser@example.com",
                Id = "Test",
                UserRole = "TestRole",
                Username = "username"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(viewModel.Id)).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.FindByNameAsync("username")).ReturnsAsync(_mockUser);

            // Act
            var result = await _controller.EditDetail(viewModel) as ViewResult;

            // Assert
            Assert.That(result!.TempData["ErrorMsg"], Is.EqualTo(Message.PROF_USERNAME_EXIST));
        }

        [Test]
        public async Task EditDetail_ValidateModelState_ModifyEmail_SendEmail()
        {
            // Arrange
            var viewModel = new EditProfileViewModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "testuser1@example.com",
                Id = "Test",
                UserRole = "TestRole",
                Username = "testuser"
            };

            string expectedCallbackUrl = "http://localhost/Profile/ChangeEmail";
            string bodyTemplate = "Account Confirmation: {UserName}, {ConfirmationLink}";

            _mockUserManager.Setup(x => x.FindByIdAsync(viewModel.Id)).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.GenerateChangeEmailTokenAsync(_mockUser, viewModel.Email)).ReturnsAsync("dummyToken");
            _urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedCallbackUrl);
            _mockEmailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockUserManager.Setup(x => x.UpdateAsync(_mockUser)).ReturnsAsync(IdentityResult.Success);

            var streamReaderMock = new MockStreamReader(bodyTemplate);
            _controller.StreamReaderFactory = path => streamReaderMock;

            // Act
            var result = await _controller.EditDetail(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.TempData["SuccessMsg"], Is.EqualTo(Message.PROF_DETAIL_EDIT));
        }

        [Test]
        public async Task ChangeEmail_NullValue()
        {
            // Arrange


            // Act
            var result = await _controller.ChangeEmail(null, null, null) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

        [Test]
        public async Task ChangeEmail_UserNotFound()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.ChangeEmail("1", "test@example.com", "dummyToken") as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

        [Test]
        public async Task ChangeEmail_TokenExpired()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.ChangeEmailAsync(_mockUser, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.ChangeEmail("1", "test@gmail.com", "dummyToken") as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

        [Test]
        public async Task ChangeEmail_Valid()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.ChangeEmailAsync(_mockUser, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ChangeEmail("1", "test@gmail.com", "dummyToken") as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

        [Test]
        public async Task CallEditPassword_ReturnEditPasswordView()
        {
            // Arrange

            // Act
            var result = await _controller.EditPassword() as ViewResult;
            var viewModel = result?.Model as EditProfilePasswordViewModel;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditPassword_InvalidModelStateReturnView()
        {
            // Arrange
            var viewModel = new EditProfilePasswordViewModel
            {
                Id = "1",
                CurrentPassword = "Test",
                NewPassword = "Testing1234!!",
                ConfirmPassword = "Testing1234!!"
            };

            _controller.ModelState.AddModelError("CurrentPassword", "Required");

            // Act
            var result = await _controller.EditPassword(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("EditPassword"));
            Assert.That(result.Model, Is.EqualTo(viewModel));
        }

        [Test]
        public async Task EditPassword_ValidModelState_PasswordMismatch_ReturnView()
        {
            // Arrange
            var viewModel = new EditProfilePasswordViewModel
            {
                Id = "1",
                CurrentPassword = "Test",
                NewPassword = "Testing1234!!",
                ConfirmPassword = "Testing1234!!"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(viewModel.Id)).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(_mockUser, viewModel.CurrentPassword, viewModel.NewPassword)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.EditPassword(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("EditPassword"));
            Assert.That(result.Model, Is.EqualTo(viewModel));
            Assert.That(result.TempData["ErrorMsg"], Is.EqualTo(Message.PROF_PASS_EDIT_FAIL));
        }

        [Test]
        public async Task EditPassword_ValidModelState_Valid()
        {
            // Arrange
            var viewModel = new EditProfilePasswordViewModel
            {
                Id = "1",
                CurrentPassword = "Test",
                NewPassword = "Testing1234!!",
                ConfirmPassword = "Testing1234!!"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(viewModel.Id)).ReturnsAsync(_mockUser);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(_mockUser, viewModel.CurrentPassword, viewModel.NewPassword)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(_mockUser)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.EditPassword(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

        [Test]
        public async Task CallEditContact_ReturnEditContactView()
        {
            // Arrange

            // Act
            var result = await _controller.EditContact() as ViewResult;
            var viewModel = result?.Model as EditContactViewModel;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditContact_InvalidModelState()
        {
            // Arrange
            var viewModel = new EditContactViewModel
            {
                PhoneNumber = "601234567890",
                UserPermissions = _mockPermission
            };

            _controller.ModelState.AddModelError("PhoneNumber", "Required");

            // Act
            var result = await _controller.EditContact(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.TempData["ErrorMsg"], Is.EqualTo(Message.COMMON_ERROR));
        }

        [Test]
        public async Task EditContact_ValidModelState()
        {
            // Arrange
            var viewModel = new EditContactViewModel
            {
                PhoneNumber = "601234567890",
                UserPermissions = _mockPermission
            };

            _mockUserManager.Setup(x => x.GenerateChangePhoneNumberTokenAsync(_mockUser, viewModel.PhoneNumber)).ReturnsAsync("dummyToken");
            _mockSmsSender.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.EditContact(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("VerifyPhoneNumber"));
            Assert.That(result.RouteValues!["phoneNumber"], Is.EqualTo(viewModel.PhoneNumber));
        }

        [Test]
        public void CallVerifyPhoneNumber_ReturnVerifyPhoneNumberView()
        {
            // Arrange
            var phoneNumber = "601234567890";

            // Act
            var result = _controller.VerifyPhoneNumber(phoneNumber) as ViewResult;
            var viewModel = result?.Model as VerifyPhoneNumberViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(viewModel!.PhoneNumber, Is.EqualTo(phoneNumber));
        }

        [Test]
        public void CallVerifyPhoneNumber_ReturnError()
        {
            // Arrange 

            // Act
            var result = _controller.VerifyPhoneNumber(phoneNumber: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewName, Is.EqualTo("Error"));
        }

        [Test]
        public async Task VerifyPhoneNumberInvalidModelState_ReturnView()
        {
            // Arrange
            var viewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "601234567890",
                Code = "123456"
            };

            _controller.ModelState.AddModelError("Code", "Code is required");


            // Act
            var result = await _controller.VerifyPhoneNumber(viewModel) as ViewResult;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Model, Is.EqualTo(viewModel));
            Assert.That(result.TempData["ErrorMsg"], Is.EqualTo(Message.PROF_CONTACT_EDIT_FAIL));
        }

        [Test]
        public async Task VerifyPhoneNumberValidModel_ReturnView()
        {
            // Arrange
            var viewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "601234567890",
                Code = "123456"
            };

            _mockUserManager.Setup(x => x.ChangePhoneNumberAsync(_mockUser, viewModel.PhoneNumber, viewModel.Code)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.VerifyPhoneNumber(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Profile"));
        }

    }

}
