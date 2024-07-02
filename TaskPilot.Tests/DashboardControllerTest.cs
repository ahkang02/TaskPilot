using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class DashboardControllerTest
    {
        private Mock<ITaskService> _taskService;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private DashboardController _dashboardController;

        [SetUp]
        public void SetUp()
        {
            _taskService = new Mock<ITaskService>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _dashboardController = new DashboardController(_userManagerMock.Object, _taskService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dashboardController.Dispose();
        }

        [Test]
        public async Task Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "1",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
            };

            var task = new Tasks
            {
                Id = Guid.NewGuid(),
                Name = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                Created = DateTime.Now,
                Priority = new Priorities { Description = "High", ColorCode = "#FF0000" },
                Status = new Statuses { Description = "Open", ColorCode = "#0000FF" },
                AssignTo = user,
                AssignFrom = user,
                AssignFromId = user.Id,
                AssignToId = user.Id,
            };

            var taskList = new List<Tasks> { task };

            _dashboardController.ControllerContext = new ControllerContext();
            _dashboardController.ControllerContext.HttpContext = new DefaultHttpContext();
            _dashboardController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }, "mock"));
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "Admin" });
            _taskService.Setup(x => x.GetNotClosedTaskSortByCreatedDateInDescFilterByUserId(It.IsAny<string>())).Returns(taskList);

            // Act
            var result = await _dashboardController.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }
    }
}
