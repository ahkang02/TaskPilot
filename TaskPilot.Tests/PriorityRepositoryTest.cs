using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;
using TaskPilot.Infrastructure.Repository;

namespace TaskPilot.Tests
{
    public class PriorityRepositoryTest
    {
        private TaskContext _context;
        private IRepository<Priorities> _priorityRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskContext(options);

            _context.Priorities.AddRange(new List<Priorities>
            {
                new Priorities {Id = new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"), Description = "High" },
                new Priorities {Id = new Guid("a35cd343-1980-4c49-a7c5-8fdae8bea020"), Description = "Medium" },
                new Priorities {Id = new Guid("c37f9705-b675-47fb-986c-a5e594b9e90d"), Description = "Low" },
                new Priorities {Id = new Guid("e2692587-0348-4455-a7c9-c3948b06c809"), Description = "Lowest" },
                new Priorities {Id = new Guid("32949b51-3358-4df5-b038-fe9062460275"), Description = "Testing" },
                new Priorities {Id = new Guid("ba978280-0c83-4f3b-b441-bb3cd6253a6f"), Description = "Testing" },

            });
            _context.SaveChanges();

            _priorityRepository = new Repository<Priorities>(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAll_ReturnAllPriorities()
        {
            var result = _priorityRepository.GetAll();
            Assert.AreEqual(6, result.Count());
            Assert.AreEqual("High", result.First().Description);
        }

        [Test]
        public void GetById_ReturnsCorrectPriority()
        {
            var result = _priorityRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            Assert.AreEqual("High", result.Description);
        }

        [Test]
        public void Find_ReturnsCorrectPriority()
        {
            var result = _priorityRepository.Find(s => s.Description == "Medium");
            Assert.AreEqual("Medium", result.First().Description);
        }

        [Test]
        public void Add_AddsPriority()
        {
            var priority = new Priorities { Id = new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"), Description = "Test" };
            _priorityRepository.Add(priority);
            _context.SaveChanges();

            var result = _priorityRepository.Get(s => s.Id == new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"));
            Assert.AreEqual("Test", result.Description);
        }

        [Test]
        public void Add_AddsRangePriorities()
        {
            var priorities = new List<Priorities>
            {
                new Priorities { Id = new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"), Description = "Test" },
                new Priorities { Id = new Guid("3eb34051-28ac-4c16-90f1-22d44e9a59ae"), Description = "Test 2"}
            };
            _priorityRepository.AddRange(priorities);
            _context.SaveChanges();

            var result = _priorityRepository.GetAll();
            Assert.AreEqual(8, result.Count());
            Assert.AreEqual("Test 2", result.ElementAt(result.Count() - 2).Description);
            Assert.AreEqual("Test", result.ElementAt(result.Count() - 1).Description);
        }

        [Test]
        public void Update_ModifiesExistingPriority()
        {
            var priority = _priorityRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            priority.Description = "Updated";
            _priorityRepository.Update(priority);
            _context.SaveChanges();

            var result = _priorityRepository.GetAll();
            Assert.AreEqual("Updated", result.First().Description);
        }

        [Test]
        public void Delete_RemovesPriority()
        {
            var priority = _priorityRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            _priorityRepository.Remove(priority);
            _context.SaveChanges();

            var result = _priorityRepository.GetAll();
            Assert.AreEqual(5, result.Count());
            Assert.AreEqual("Medium", result.First().Description);
        }

        [Test]
        public void Delete_RemoveRangeStatuses()
        {
            var status = _priorityRepository.Find(s => s.Description == "Testing");
            _priorityRepository.RemoveRange(status);
            _context.SaveChanges();

            var result = _priorityRepository.GetAll();
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("Lowest", result.Last().Description);
        }
    }
}
