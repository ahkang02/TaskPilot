using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class TaskServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private ITaskService _taskService;
        private List<Statuses> _mockStatuses;
        private List<Tasks> _mockTasks;
        private List<Priorities> _mockPriorities;
        private List<ApplicationUser> _mockUsers;


        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _taskService = new TaskService(_mockUnitOfWork.Object);

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
                new ApplicationUser { Id = "user3", UserName = "User Three", FirstName = "Test" , LastName = "3" },
                new ApplicationUser { Id = "user4", UserName = "User Three", FirstName = "Test" , LastName = "3" },
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

            _mockUnitOfWork.Setup(u => u.Status.GetAll()).Returns(_mockStatuses.AsQueryable());
            _mockUnitOfWork.Setup(u => u.Priority.GetAll()).Returns(_mockPriorities.AsQueryable());
            _mockUnitOfWork.Setup(u => u.Users.GetAll()).Returns(_mockUsers.AsQueryable());
            _mockUnitOfWork.Setup(u => u.Tasks.GetAll()).Returns(_mockTasks.AsQueryable());
            _mockUnitOfWork.Setup(u => u.Tasks.GetAllInclude(It.IsAny<Expression<Func<Tasks, bool>>>(), "Status,Priority,AssignFrom,AssignTo")).Returns(_mockTasks);

        }

        [Test]
        public void GetAllNotClosedTaskSortByCreatedDateInDescFilterByUserId_ShouldReturnAllNotClosedTasks()
        {
            _mockUnitOfWork.Setup(u => u.Tasks.GetAllInclude(It.IsAny<Expression<Func<Tasks, bool>>>(), "Status,Priority,AssignFrom,AssignTo")).Returns((Expression<Func<Tasks, bool>> predicate, string includes) =>
_mockTasks.AsQueryable().Where(predicate));

            var result = _taskService.GetNotClosedTaskSortByCreatedDateInDescFilterByUserId("user1");
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetAllTaskWithInclude_ShouldReturnAllTasks()
        {
            var result = _taskService.GetAllTaskWithInclude();
            Assert.That(result.Count(), Is.EqualTo(4));
        }

        [Test]
        public void IsUserHoldingTask_ShouldReturnTrue()
        {
            var result = _taskService.IsUserHoldingTask("user1");
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsUserHoldingTask_ShouldReturnFalse()
        {
            var result = _taskService.IsUserHoldingTask("user4");
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetTasksWithId_ShouldReturnTask()
        {
            _mockUnitOfWork.Setup(u => u.Tasks.GetAllInclude(It.IsAny<Expression<Func<Tasks, bool>>>(), "Status,Priority,AssignFrom,AssignTo")).Returns((Expression<Func<Tasks, bool>> predicate, string includes) =>
_mockTasks.AsQueryable().Where(predicate));

            var result = _taskService.GetTasksWithId(_mockTasks[0].Id.Value);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetTaskWithId_ShouldReturnNull()
        {
            _mockUnitOfWork.Setup(u => u.Tasks.GetAllInclude(It.IsAny<Expression<Func<Tasks, bool>>>(), "Status,Priority,AssignFrom,AssignTo")).Returns((Expression<Func<Tasks, bool>> predicate, string includes) =>
_mockTasks.AsQueryable().Where(predicate));

            var result = _taskService.GetTasksWithId(Guid.NewGuid());
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CreateTask_ShouldCreateTask()
        {
            var task = new Tasks
            {
                Id = Guid.NewGuid(),
                Name = "Task 5",
                Description = "Description for Task 5",
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
            };

            _taskService.CreateTask(task);
            _mockUnitOfWork.Verify(u => u.Tasks.Add(task), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void UpdateTask_ShouldUpdateTask()
        {
            var task = _mockTasks[0];
            task.Name = "Task 1 Updated";
            task.Description = "Description for Task 1 Updated";

            _taskService.UpdateTask(task);
            _mockUnitOfWork.Verify(u => u.Tasks.Update(task), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteTask_ShouldDeleteTask()
        {
            var task = _mockTasks[0];
            _taskService.DeleteTask(task);
            _mockUnitOfWork.Verify(u => u.Tasks.Remove(task), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void AddRangeTasks_ShouldAddRangeTasks()
        {
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    Id = Guid.NewGuid(),
                    Name = "Task 5",
                    Description = "Description for Task 5",
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
                    Name = "Task 6",
                    Description = "Description for Task 6",
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
                }
            };

            _taskService.AddRangeTasks(tasks);
            _mockUnitOfWork.Verify(u => u.Tasks.AddRange(tasks), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteRangeTasks_ShouldDeleteRangeTasks()
        {
            var tasks = new List<Tasks>
            {
                _mockTasks[0],
                _mockTasks[1]
            };

            _taskService.DeleteRangeTasks(tasks);
            _mockUnitOfWork.Verify(u => u.Tasks.RemoveRange(tasks), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
    }
}
