using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Data;
using TaskPilot.Infrastructure.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class RepositoryTest
    {
        private Mock<TaskContext> _mockContext;
        private Mock<DbSet<TestEntity>> _mockDbSet;
        private IRepository<TestEntity> _repository;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<TaskContext>();
            _mockDbSet = new Mock<DbSet<TestEntity>>();

            var data = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Test 1" },
                new TestEntity { Id = 2, Name = "Test 2" },
                new TestEntity { Id = 2, Name = "Test" },
                new TestEntity { Id = 2, Name = "Test" },
            }.AsQueryable();

            _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.Provider).Returns(data.Provider);
            _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.Expression).Returns(data.Expression);
            _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
            _mockDbSet.As<IQueryable<TestEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            // Setup behavior for Add, Update, Remove methods
            _mockDbSet.Setup(m => m.Add(It.IsAny<TestEntity>())).Verifiable();
            _mockDbSet.Setup(m => m.Update(It.IsAny<TestEntity>())).Verifiable();
            _mockDbSet.Setup(m => m.Remove(It.IsAny<TestEntity>())).Verifiable();
            
            _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
            _repository = new Repository<TestEntity>(_mockContext.Object);
            _unitOfWork = new UnitOfWork(_mockContext.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public void Save_ShouldCallSaveChangesOnContext()
        {
            _unitOfWork.Save();
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Test]
        public void GetAll_ReturnsAllEntities()
        {
            var result = _repository.GetAll();
            Assert.That(result.Count(), Is.EqualTo(4));
            Assert.That(result.First().Name, Is.EqualTo("Test 1"));
        }

        [Test]
        public void GetById_ReturnsCorrectEntity()
        {
            var result = _repository.Get(t => t.Id == 1);
            Assert.That(result.Name, Is.EqualTo("Test 1"));
        }

        [Test]
        public void AddEntity_ShouldAddEntityToContext()
        {
            var entity = new TestEntity { Id = 3, Name = "Test 3" };
            _repository.Add(entity);
            _mockDbSet.Verify(m => m.Add(entity), Times.Once());
        }

        [Test]
        public void AddRangeEntity_ShouldAddEntityToContext()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 3, Name = "Test 3" },
                new TestEntity { Id = 4, Name = "Test 4" },
            };
            _repository.AddRange(entities);
            _mockDbSet.Verify(m => m.AddRange(entities), Times.Once());
        }

        [Test]
        public void UpdateEntity_ShouldUpdateEntityInContext()
        {
            // Arrange
            var entityToUpdate = _repository.Get(t => t.Id == 1);
            entityToUpdate.Name = "Updated Test 1";

            _repository.Update(entityToUpdate);

            _mockDbSet.Verify(m => m.Update(entityToUpdate), Times.Once());
        }

        [Test]
        public void FindEntity_ShouldReturnListOfEntities()
        {
            var result = _repository.Find(t => t.Name == "Test");
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void DeleteEntity_ShouldRemoveEntityToContext()
        {

           var entityToDelete = _repository.Get(t => t.Id == 1);
            _repository.Remove(entityToDelete);
            _mockDbSet.Verify(m => m.Remove(entityToDelete), Times.Once());
        }

        [Test]
        public void DeleteEntityRange_ShouldRemoveEntityToContext()
        {
            var entitiesToDelete = _repository.GetAll();
            _repository.RemoveRange(entitiesToDelete);
            _mockDbSet.Verify(m => m.RemoveRange(entitiesToDelete), Times.Once());
        }
    
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
