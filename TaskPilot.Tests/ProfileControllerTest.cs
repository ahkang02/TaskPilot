using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
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
            var viewModel = new EditProfileViewModel {
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
    }

}
