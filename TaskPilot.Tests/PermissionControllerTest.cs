using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class PermissionControllerTest
    {
        private Mock<IPermissionService> _mockPermissionService;
        private Mock<IFeatureService> _mockFeatureService;
        private PermissionController _permissionController;

        [SetUp]
        public void Setup()
        {
            _mockPermissionService = new Mock<IPermissionService>();
            _mockFeatureService = new Mock<IFeatureService>();
            _permissionController = new PermissionController(_mockPermissionService.Object, _mockFeatureService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _permissionController.Dispose();
        }

        [Test]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Act
            var result = _permissionController.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void New_WhenCalled_ReturnsViewResult()
        {
            // Act
            var result = _permissionController.New();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void New_WhenCalled_ReturnsViewResultWithViewModel()
        {
            // Arrange
            var viewModel = new EditPermissionViewModel
            {
                Features = new List<Features>()
            };

            _mockFeatureService.Setup(x => x.GetAllFeatures()).Returns(viewModel.Features);

            // Act
            var result = _permissionController.New();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void New_WhenModelStateIsValid_ReturnsRedirectToAction()
        {
            // Arrange
            var viewModel = new EditPermissionViewModel
            {
                Id = null,
                Name = "Test Permission",
                FeatureId = Guid.NewGuid()
            };

            var mockFeatures = new Features
            {
                Name = "Test",
                Id = Guid.NewGuid()
            };

            _permissionController.ControllerContext = new ControllerContext();
            _permissionController.ControllerContext.HttpContext = new DefaultHttpContext();
            _permissionController.TempData = new Mock<ITempDataDictionary>().Object;

            _mockFeatureService.Setup(x => x.GetFeaturesById(viewModel.FeatureId)).Returns(mockFeatures);
            _mockPermissionService.Setup(x => x.CreatePermission(new Permission { FeaturesId = viewModel.FeatureId, Name = viewModel.Name, Features = mockFeatures }));

            // Act
            var result = _permissionController.New(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void New_WhenModelStateIsInvalid_ReturnsToView()
        {
            // Arrange
            var viewModel = new EditPermissionViewModel
            {
                Id = null,
                Name = "Test Permission",
                FeatureId = Guid.NewGuid()
            };


            var mockHttpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            _permissionController.ControllerContext = controllerContext;

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                ["SuccessMsg"] = null
            };
            _permissionController.TempData = tempData;
            _permissionController.ModelState.AddModelError("Test", "Testing");

            // Act
            var result = _permissionController.New(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void New_WhenIdIsNotNull_Update_ReturnRedirectToAction()
        {
            // Arrange
            var viewModel = new EditPermissionViewModel
            {
                Id = Guid.NewGuid(),
                Name = "Test Permission",
                FeatureId = Guid.NewGuid()
            };

            var mockFeatures = new Features
            {
                Name = "Test",
                Id = Guid.NewGuid()
            };

            var mockPermission = new Permission
            {

                Id = viewModel.Id.Value,
                Name = viewModel.Name,
                Features = mockFeatures,
                FeaturesId = mockFeatures.Id
            };

            _permissionController.ControllerContext = new ControllerContext();
            _permissionController.ControllerContext.HttpContext = new DefaultHttpContext();
            _permissionController.TempData = new Mock<ITempDataDictionary>().Object;

            _mockPermissionService.Setup(x => x.GetPermissionById(viewModel.Id.Value)).Returns(mockPermission);
            _mockPermissionService.Setup(x => x.UpdatePermission(new Permission
            {
                FeaturesId = viewModel.FeatureId,
                Name = viewModel.Name,
                Features = mockFeatures
            }));

            // Act
            var result = _permissionController.New(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }
    }
}
