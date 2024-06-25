using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class StatusServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private IStatusService _statusService;
        private List<Statuses> _mockStatuses;
        private List<Tasks> _mockTasks;
        private List<Priorities> _mockPriorities;
        private List<ApplicationUser> _mockUsers;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _statusService = new StatusService(_mockUnitOfWork.Object);

            _mockStatuses = new List<Statuses>
            {
                new Statuses
                {
                    Id = new Guid("8edc5761-141d-495a-8260-4d2cb5bc788b"),
                    Description =  "Test Status 1",
                    ColorCode = "#000000"
                },
                new Statuses
                {
                    Id = new Guid("52808e75-3202-4482-9e3a-619bb9bd59ed"),
                    Description =  "Test Status 2",
                    ColorCode = "#000000"
                },
                new Statuses
                {
                    Id = new Guid("b0b9bf9e-3211-4dc3-8318-75046f451117"),
                    Description =  "Test Status 3",
                    ColorCode = "#000000"
                },
                new Statuses
                {
                    Id = new Guid("1eb0a1b1-ba81-4d7a-9b1f-fdd0c5b7ae42"),
                    Description =  "Test Status 4",
                    ColorCode = "#000000"
                },
            };

            _mockPriorities = new List<Priorities>
            {
                new Priorities { Id = Guid.NewGuid(), Description = "Low" , ColorCode = "#000000"},
                new Priorities { Id = Guid.NewGuid(), Description = "Medium", ColorCode = "#000000"},
                new Priorities { Id = Guid.NewGuid(), Description = "High", ColorCode ="#000000"}
            };

            _mockUsers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", UserName = "User One", FirstName = "Test" , LastName = "1" },
                new ApplicationUser { Id = "user2", UserName = "User Two", FirstName = "Test" , LastName = "2" },
                new ApplicationUser { Id = "user3", UserName = "User Three", FirstName = "Test" , LastName = "3" }
            };

            _mockTasks = new List<Tasks>
            {
                new Tasks
                    {
                        Id = Guid.NewGuid(),
                        Name = "Task 1",
                        Description = "Description for Task 1",
                        Priority = _mockPriorities[0],
                        PriorityId = _mockPriorities[0].Id,
                        Status = _mockStatuses[0],
                        StatusId = _mockStatuses[0].Id,
                        DueDate = DateTime.Now.AddDays(7),
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        AssignTo = _mockUsers[0],
                        AssignToId = _mockUsers[0].Id,
                        AssignFrom = _mockUsers[1],
                        AssignFromId = _mockUsers[1].Id,
                        DependencyId = null
                    },
                    new Tasks
                    {
                        Id = Guid.NewGuid(),
                        Name = "Task 2",
                        Description = "Description for Task 2",
                        Priority = _mockPriorities[1],
                        PriorityId = _mockPriorities[1].Id,
                        Status = _mockStatuses[1],
                        StatusId = _mockStatuses[1].Id,
                        DueDate = DateTime.Now.AddDays(14),
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        AssignTo = _mockUsers[1],
                        AssignToId = _mockUsers[1].Id,
                        AssignFrom = _mockUsers[2],
                        AssignFromId = _mockUsers[2].Id,
                        DependencyId = null
                    },
                    new Tasks
                    {
                        Id = Guid.NewGuid(),
                        Name = "Task 3",
                        Description = "Description for Task 3",
                        Priority = _mockPriorities[2],
                        PriorityId = _mockPriorities[2].Id,
                        Status = _mockStatuses[1],
                        StatusId = _mockStatuses[1].Id,
                        DueDate = DateTime.Now.AddDays(21),
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        AssignTo = _mockUsers[2],
                        AssignToId = _mockUsers[2].Id,
                        AssignFrom = _mockUsers[0],
                        AssignFromId = _mockUsers[0].Id,
                        DependencyId = null
                    },
                    new Tasks
                    {
                        Id = Guid.NewGuid(),
                        Name = "Task 4",
                        Description = "Description for Task 4",
                        Priority = _mockPriorities[1],
                        PriorityId = _mockPriorities[1].Id,
                        Status = _mockStatuses[2],
                        StatusId = _mockStatuses[2].Id,
                        DueDate = DateTime.Now.AddDays(28),
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        AssignTo = _mockUsers[0],
                        AssignToId = _mockUsers[0].Id,
                        AssignFrom = _mockUsers[2],
                        AssignFromId = _mockUsers[2].Id,
                        DependencyId = null
                    }
            };

            // Set up mock methods
            _mockUnitOfWork.Setup(u => u.Status.GetAll()).Returns(_mockStatuses);
            _mockUnitOfWork.Setup(u => u.Tasks.GetAll()).Returns(_mockTasks);

            _statusService = new StatusService(_mockUnitOfWork.Object);
        }

        [Test]
        public void CreateStatus_ShouldAddStatus()
        {
            // Arrange
            var status = new Statuses { Id = Guid.NewGuid(), Description = "New Status", ColorCode = "#000000" };

            // Act
            _statusService.CreateStatus(status);

            // Assert
            _mockUnitOfWork.Verify(u => u.Status.Add(status), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteStatus_ShouldRemoveStatus()
        {
            //Arrange
            var status = _statusService.GetStatusByName("Test Status 1");

            //Act
            _statusService.DeleteStatus(status);

            //Assert
            _mockUnitOfWork.Verify(u => u.Status.Remove(status), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void GetAllStatuses_ShouldReturnAllStatuses()
        {
            // Act
            var result = _statusService.GetAllStatuses();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(_mockStatuses.Count));
            Assert.That(result, Is.EqualTo(_mockStatuses));
        }

        [Test]
        public void GetStatusById_ShouldReturnCorrectStatus()
        {
            // Arrange
            var statusId = _mockStatuses[1].Id;
            _mockUnitOfWork.Setup(u => u.Status.Get(s => s.Id == statusId)).Returns(_mockStatuses[1]);

            // Act
            var result = _statusService.GetStatusById(statusId);

            // Assert
            Assert.That(result, Is.EqualTo(_mockStatuses[1]));
        }

        [Test]
        public void GetStatusById_ShouldReturnNull_WhenStatusNotFound()
        {
            // Arrange
            var statusId = new Guid("b0b9bf9e-3211-4dc3-8318-75046f451111");

            // Act
            var result = _statusService.GetStatusById(statusId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStatusByName_ShouldReturnCorrectStatus()
        {
            // Arrange
            var statusName = "Test Status 1";
            _mockUnitOfWork.Setup(u => u.Status.Get(s => s.Description == statusName)).Returns(_mockStatuses[0]);

            // Act
            var result = _statusService.GetStatusByName(statusName);

            // Assert
            Assert.That(result, Is.EqualTo(_mockStatuses[0]));
        }

        [Test]
        public void GetStatusByName_ShouldReturnNull_WhenStatusNotFound()
        {
            // Arrange
            var statusName = "Non-existent Status";
            _mockUnitOfWork.Setup(u => u.Status.Get(s => s.Description == statusName)).Returns((Statuses)null);

            // Act
            var result = _statusService.GetStatusByName(statusName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void UpdateStatus_ShouldUpdateStatus()
        {
            // Arrange
            var statusName = "Test Status 1";
            _mockUnitOfWork.Setup(u => u.Status.Get(s => s.Description == statusName)).Returns(_mockStatuses[0]);

            var statusToUpdate = _statusService.GetStatusByName(statusName);

            statusToUpdate.Description = "Updated Status";

            // Act
            _statusService.UpdateStatus(statusToUpdate);

            // Assert
            _mockUnitOfWork.Verify(u => u.Status.Update(statusToUpdate), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CheckIfStatusIsInUse_ShouldReturnTrue_WhenStatusIsInUse()
        {
            // Arrange
            var status = _mockStatuses[0];

            // Act
            var result = _statusService.CheckIfStatusIsInUse(status);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckIfStatusIsInUse_ShouldReturnFalse_WhenStatusIsNotInUse()
        {
            // Arrange
            var status = _mockStatuses[3];

            // Act
            var result = _statusService.CheckIfStatusIsInUse(status);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
