using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class PriorityControllerTest
    {
        private Mock<IPriorityService> _priorityService;
        private Mock<IUserPermissionService> _userPermissionService;
        private PriorityController _priorityController;
        private Mock<IUrlHelper> _urlHelper;

        [SetUp]
        public void Setup()
        {
            _priorityService = new Mock<IPriorityService>();
            _userPermissionService = new Mock<IUserPermissionService>();
            _priorityController = new PriorityController(_priorityService.Object, _userPermissionService.Object);

            _userPermissionService.Setup(x => x.GetUserPermission(It.IsAny<ClaimsIdentity>())).Returns(new List<Permission>());
            _priorityController.ControllerContext.HttpContext = new DefaultHttpContext();
            _priorityController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }, "mock"));
            _priorityController.TempData = new Mock<ITempDataDictionary>().Object;
            _urlHelper = new Mock<IUrlHelper>();
            _priorityController.Url = _urlHelper.Object;

        }

        [TearDown]
        public void TearDown()
        {
            _priorityController.Dispose();
        }

        [Test]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity();

            // Act
            var result = _priorityController.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void New_WhenCalled_ReturnsViewResult()
        {
            // Act
            var result = _priorityController.New();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void New_CreateNewPriority_ReturnsViewResult()
        {
            // Arrange
            var viewModel = new EditPriorityViewModel
            {
                Name = "Test Priority",
                ColorCode = "#FF0000"
            };

            // Act
            var result = _priorityController.New(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void New_UpdatePriority_ReturnsViewResult()
        {
            // Arrange
            var viewModel = new EditPriorityViewModel
            {
                Id = Guid.NewGuid(),
                Name = "Test Priority",
                ColorCode = "#FF0000"
            };

            var priority = new Priorities
            {
                Id = viewModel.Id.Value,
                Description = viewModel.Name,
                ColorCode = viewModel.ColorCode
            };

            _priorityService.Setup(x => x.GetPrioritiesById(viewModel.Id.Value)).Returns(priority);
            _priorityService.Setup(x => x.UpdatePriority(priority));

            // Act
            var result = _priorityController.New(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void New_InvalidModelState_ReturnsView()
        {

            // Arrange
            var viewModel = new EditPriorityViewModel
            {
                Id = null,
                Name = "Test Priority",
                ColorCode = "#FF0000"
            };

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                ["SuccessMsg"] = null
            };

            _priorityController.TempData = tempData;
            _priorityController.ModelState.AddModelError("Test", "Testing");

            // Act
            var result = _priorityController.New(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.That(result.TempData["ErrorMsg"], Is.EqualTo(Message.COMMON_ERROR));
        }

        [Test]
        public void Update_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var priority = new Priorities
            {
                Id = Guid.NewGuid(),
                Description = "Test Priority",
                ColorCode = "#FF0000"
            };

            _priorityService.Setup(x => x.GetPrioritiesByName("Test Priority")).Returns(priority);

            // Act
            var result = _priorityController.Update("Test Priority") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.That(result!.ViewName, Is.EqualTo("New"));
        }

        [Test]
        public void Delete_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var priority = new Priorities
            {
                Id = Guid.NewGuid(),
                Description = "Test Priority",
                ColorCode = "#FF0000"
            };

            _priorityService.Setup(x => x.GetPrioritiesById(priority.Id)).Returns(priority);
            _priorityService.Setup(x => x.CheckIfPriorityIsInUse(priority)).Returns(false);
            _priorityService.Setup(x => x.DeletePriority(priority));
            _urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("Index");

            // Act
            var result = _priorityController.Delete(new Guid[] { priority.Id }) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            Assert.That(result.Value, Is.EqualTo("Index"));
        }

        [Test]
        public void DeleteWhenPriorityInUse_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var priority = new Priorities
            {
                Id = Guid.NewGuid(),
                Description = "Test Priority",
                ColorCode = "#FF0000"
            };

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                ["SuccessMsg"] = null
            };

            _priorityController.TempData = tempData;
            _priorityService.Setup(x => x.GetPrioritiesById(priority.Id)).Returns(priority);
            _priorityService.Setup(x => x.CheckIfPriorityIsInUse(priority)).Returns(true);
            _priorityService.Setup(x => x.DeletePriority(priority));
            _urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("Index");

            // Act
            var result = _priorityController.Delete(new Guid[] { priority.Id }) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            Assert.That(result.Value, Is.EqualTo("Index"));
        }
    }
}
