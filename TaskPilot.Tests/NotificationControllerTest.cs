using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.Controllers;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class NotificationControllerTest
    {
        private Mock<INotificationService> _mockNotificationService;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private NotificationController _notificationController;

        [SetUp]
        public void Setup()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _notificationController = new NotificationController(_mockNotificationService.Object, _mockUserManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _notificationController.Dispose();
        }

        [Test]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Act
            var result = _notificationController.Index();


            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void UpdateStatusReadWithNullTaskId_ReturnRedirectRefresh()
        {
            // Arrange
            var notif = new Notifications
            {
                Id = Guid.NewGuid(),
                Description = "Test Notification",
                Status = "Unread",
                UserId = "1",
                TasksId = null
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new HeaderDictionary
        {
            { "Referer", "/" }
        };

            mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object,
            };

            _mockNotificationService.Setup(x => x.GetNotificationById(notif.Id)).Returns(notif);
            _mockNotificationService.Setup(x => x.DeleteNotification(notif));

            // Act
            var result = _notificationController.UpdateStatusRead(notif.Id, null) as RedirectResult;

            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.That(result!.Url, Is.EqualTo("/"));
        }

        [Test]
        public void UpdateStatusReadWithTaskId_ReturnRedirectToDetail()
        {
            // Arrange
            var notif = new Notifications
            {
                Id = Guid.NewGuid(),
                Description = "Test Notification",
                Status = "Unread",
                UserId = "1",
                TasksId = Guid.NewGuid()
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new HeaderDictionary
        {
            { "Referer", "/" }
        };

            mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object,
            };

            _mockNotificationService.Setup(x => x.GetNotificationById(notif.Id)).Returns(notif);
            _mockNotificationService.Setup(x => x.DeleteNotification(notif));

            // Act
            var result = _notificationController.UpdateStatusRead(notif.Id, notif.TasksId) as RedirectToActionResult;

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.Multiple(() =>
            {
                Assert.That(result!.ActionName, Is.EqualTo("Detail"));
                Assert.That(result.ControllerName, Is.EqualTo("Task"));
                Assert.That(result.RouteValues!["id"], Is.EqualTo(notif.TasksId));
            });
        }

        [Test]
        public void ReadAll_ClearAllNotif_RefreshPage()
        {
            // Arrange
            var notif = new List<Notifications>
        {
            new Notifications
            {
                Id = Guid.NewGuid(),
                Description = "Test Notification",
                Status = "Unread",
                UserId = "1",
                TasksId = null
            }
        };

            // Mock HttpContext and Claims
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new Mock<ClaimsIdentity>();
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new HeaderDictionary
        {
            { "Referer", "/" }
        };

            mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            // Add other claims as needed
        };

            mockIdentity.SetupGet(i => i.Claims).Returns(claims);

            var principal = new ClaimsPrincipal(mockIdentity.Object);

            mockHttpContext.SetupGet(h => h.User).Returns(principal);

            // Assign the mocked HttpContext to ControllerContext of _notificationController
            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock users data
            var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", FirstName = "Test", LastName = "User", UserName = "testuser" },
            new ApplicationUser { Id = "2", FirstName = "Test", LastName = "User" }
        }.AsQueryable();

            _mockUserManager.Setup(x => x.Users).Returns(users);

            _mockNotificationService.Setup(x => x.GetNotificationByUserId(It.IsAny<string>())).Returns(notif);
            _mockNotificationService.Setup(x => x.DeleteAllNotification(notif));

            // Act
            var result = _notificationController.ReadAll() as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Url, Is.EqualTo("/"));
        }
    }
}
