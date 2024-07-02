using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class RoleControllerTest
    {
        private Mock<IUserPermissionService> _mockUserPermissionService;
        private Mock<IPermissionService> _mockPermissionService;
        private Mock<IFeatureService> _mockFeatureService;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private RoleController _roleController;

        [SetUp]
        public void Setup()
        {
            _mockUserPermissionService = new Mock<IUserPermissionService>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockFeatureService = new Mock<IFeatureService>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(Mock.Of<IRoleStore<ApplicationRole>>(), null, null, null, null);

            _roleController = new RoleController(_mockUserManager.Object, _mockRoleManager.Object, _mockUserPermissionService.Object, _mockPermissionService.Object, _mockFeatureService.Object);
            _roleController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            _roleController.TempData = new Mock<ITempDataDictionary>().Object;
            _roleController.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());


        }


        [TearDown]
        public void TearDown()
        {
            _roleController.Dispose();
        }

        [Test]
        public void CallIndex_ReturnsViewResult()
        {

            var result = _roleController.Index();

            Assert.IsInstanceOf<ViewResult>(result);
        }

    }
}
