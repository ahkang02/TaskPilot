using Moq;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class PriorityServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private IPriorityService _priorityService;
        private List<Statuses> _mockStatuses;
        private List<Tasks> _mockTasks;
        private List<Priorities> _mockPriorities;
        private List<ApplicationUser> _mockUsers;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _priorityService = new PriorityService(_mockUnitOfWork.Object);

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
                new Priorities { Id = new Guid("8edc5761-141d-495a-8260-4d2cb5bc788b"), Description = "Low" , ColorCode = "#000000"},
                new Priorities { Id = new Guid("52808e75-3202-4482-9e3a-619bb9bd59ed"), Description = "Medium", ColorCode = "#000000"},
                new Priorities { Id = new Guid("b0b9bf9e-3211-4dc3-8318-75046f451117"), Description = "High", ColorCode ="#000000"},
                new Priorities { Id = new Guid("1eb0a1b1-ba81-4d7a-9b1f-fdd0c5b7ae42"), Description = "Lowest", ColorCode ="#000000"}
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
            _mockUnitOfWork.Setup(u => u.Priority.GetAll()).Returns(_mockPriorities);

            _priorityService = new PriorityService(_mockUnitOfWork.Object);
        }

        [Test]
        public void CreatePriority_ShouldAddPriority()
        {
            // Arrange
            var priority = new Priorities { Id = Guid.NewGuid(), Description = "New Priority", ColorCode = "#000000" };

            // Act
            _priorityService.CreatePriority(priority);

            // Assert
            _mockUnitOfWork.Verify(u => u.Priority.Add(priority), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeletePriority_ShouldRemovePriority()
        {
            //Arrange
            var priority = _priorityService.GetPrioritiesByName("Low");

            //Act
            _priorityService.DeletePriority(priority);

            //Assert
            _mockUnitOfWork.Verify(u => u.Priority.Remove(priority), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void GetAllPriority_ShouldReturnAllPriorities()
        {
            // Act
            var result = _priorityService.GetAllPriority();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(_mockPriorities.Count));
            Assert.That(result, Is.EqualTo(_mockPriorities));
        }

        [Test]
        public void GetPriorityById_ShouldReturnCorrectPriority()
        {
            // Arrange
            var priorityId = _mockPriorities[1].Id;
            _mockUnitOfWork.Setup(u => u.Priority.Get(s => s.Id == priorityId)).Returns(_mockPriorities[1]);

            // Act
            var result = _priorityService.GetPrioritiesById(priorityId);

            // Assert
            Assert.That(result, Is.EqualTo(_mockPriorities[1]));
        }

        [Test]
        public void GetPriorityById_ShouldReturnNull_WhenPriorityNotFound()
        {
            // Arrange
            var priorityId = new Guid("b0b9bf9e-3211-4dc3-8318-75046f451111");

            // Act
            var result = _priorityService.GetPrioritiesById(priorityId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPriorityByName_ShouldReturnCorrectPriority()
        {
            // Arrange
            var priorityName = "High";
            _mockUnitOfWork.Setup(u => u.Priority.Get(s => s.Description == priorityName)).Returns(_mockPriorities[2]);

            // Act
            var result = _priorityService.GetPrioritiesByName(priorityName);

            // Assert
            Assert.That(result, Is.EqualTo(_mockPriorities[2]));
        }

        [Test]
        public void GetPriorityByName_ShouldReturnNull_WhenPriorityNotFound()
        {
            // Arrange
            var priorityName = "Non-existent Priority";
            _mockUnitOfWork.Setup(u => u.Priority.Get(s => s.Description == priorityName)).Returns((Priorities)null);

            // Act
            var result = _priorityService.GetPrioritiesByName(priorityName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void UpdatePriority_ShouldUpdatePriority()
        {
            // Arrange
            var priorityName = "High";
            _mockUnitOfWork.Setup(u => u.Priority.Get(s => s.Description == priorityName)).Returns(_mockPriorities[2]);

            var priorityToUpdate = _priorityService.GetPrioritiesByName(priorityName);

            priorityToUpdate.Description = "Updated Status";

            // Act
            _priorityService.UpdatePriority(priorityToUpdate);

            // Assert
            _mockUnitOfWork.Verify(u => u.Priority.Update(priorityToUpdate), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CheckIfPriorityIsInUse_ShouldReturnTrue_WhenPriorityIsInUse()
        {
            // Arrange
            var priority = _mockPriorities[0];

            // Act
            var result = _priorityService.CheckIfPriorityIsInUse(priority);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckIfPriorityIsInUse_ShouldReturnFalse_WhenPriorityIsNotInUse()
        {
            // Arrange
            var priority = _mockPriorities[3];

            // Act
            var result = _priorityService.CheckIfPriorityIsInUse(priority);

            // Assert
            Assert.IsFalse(result);
        }

    }
}
