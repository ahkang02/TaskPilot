using Microsoft.EntityFrameworkCore;
using Moq;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Data;
using TaskPilot.Infrastructure.Repository;

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
        public void GetById_ReturnsNullIfEntityNotFound()
        {
            var result = _repository.Get(t => t.Id == 5);
            Assert.That(result, Is.Null);
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
        public void AddNullEntity_ShouldThrowArgumentNullException()
        {
            TestEntity entity = null;
            Assert.Throws<ArgumentNullException>(() => _repository.Add(entity));
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
        public void AddRangeNullEntity_ShouldThrowArgumentNullException()
        {
            IEnumerable<TestEntity> entities = null;
            Assert.Throws<ArgumentNullException>(() => _repository.AddRange(entities));
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
        public void UpdateNullEntity_ShouldReturnNull()
        {
            TestEntity entity = null;
            Assert.Throws<ArgumentNullException>(() => _repository.Update(entity));
        }

        [Test]
        public void FindEntity_ShouldReturnListOfEntities()
        {
            var result = _repository.Find(t => t.Name == "Test");
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void FindEntity_ShouldReturnEmptyListIfNoEntityFound()
        {
            var result = _repository.Find(t => t.Name == "Test 3");
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void DeleteEntity_ShouldRemoveEntityToContext()
        {

            var entityToDelete = _repository.Get(t => t.Id == 1);
            _repository.Remove(entityToDelete);
            _mockDbSet.Verify(m => m.Remove(entityToDelete), Times.Once());
        }

        [Test]
        public void DeleteNullEntity_ShouldThrowArgumentNullException()
        {
            var entityToDelete = _repository.Get(t => t.Id == 5);
            Assert.Throws<ArgumentNullException>(() => _repository.Remove(entityToDelete));
        }

        [Test]
        public void DeleteEntityRange_ShouldRemoveEntityToContext()
        {
            var entitiesToDelete = _repository.GetAll();
            _repository.RemoveRange(entitiesToDelete);
            _mockDbSet.Verify(m => m.RemoveRange(entitiesToDelete), Times.Once());
        }

        [Test]
        public void DeleteNullEntityRange_ShouldThrowArgumentNullException()
        {
            IEnumerable<TestEntity> entitiesToDelete = null; // Simulate no entities found
            Assert.Throws<ArgumentNullException>(() => _repository.RemoveRange(entitiesToDelete));
        }

    }

    public class TestEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
