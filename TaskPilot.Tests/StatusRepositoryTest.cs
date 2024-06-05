using Microsoft.EntityFrameworkCore;
using Moq;
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
    [TestFixture]
    public class StatusRepositoryTest
    {
        private TaskContext _context;
        private IRepository<Statuses> _statusRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskContext(options);

            _context.Statuses.AddRange(new List<Statuses>
            {
                new Statuses {Id = new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"), Description = "New" },
                new Statuses {Id = new Guid("a35cd343-1980-4c49-a7c5-8fdae8bea020"), Description = "In-Progress" },
                new Statuses {Id = new Guid("c37f9705-b675-47fb-986c-a5e594b9e90d"), Description = "Closed" },
                new Statuses {Id = new Guid("e2692587-0348-4455-a7c9-c3948b06c809"), Description = "Resolved" },
                new Statuses {Id = new Guid("32949b51-3358-4df5-b038-fe9062460275"), Description = "Testing" },
                new Statuses {Id = new Guid("ba978280-0c83-4f3b-b441-bb3cd6253a6f"), Description = "Testing" },

            });
            _context.SaveChanges();

            _statusRepository = new Repository<Statuses>(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAll_ReturnAllStatuses()
        {
            var result = _statusRepository.GetAll();
            Assert.AreEqual(6, result.Count());
            Assert.AreEqual("New", result.First().Description);
        }

        [Test]
        public void GetById_ReturnsCorrectStatus()
        {
            var result = _statusRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            Assert.AreEqual("New", result.Description);
        }

        [Test]
        public void Find_ReturnsCorrectStatus()
        {
            var result = _statusRepository.Find(s => s.Description == "In-Progress");
            Assert.AreEqual("In-Progress", result.First().Description);
        }

        [Test]
        public void Add_AddsStatus()
        {
            var status = new Statuses { Id = new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"), Description = "Test" };
            _statusRepository.Add(status);
            _context.SaveChanges();

            var result = _statusRepository.Get(s => s.Id == new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"));
            Assert.AreEqual("Test", result.Description);
        }

        [Test]
        public void Add_AddsRangeStatus()
        {
            var statuses = new List<Statuses> 
            {
                new Statuses { Id = new Guid("f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"), Description = "Test" },
                new Statuses { Id = new Guid("3eb34051-28ac-4c16-90f1-22d44e9a59ae"), Description = "Test 2"}
            };
            _statusRepository.AddRange(statuses);
            _context.SaveChanges();

            var result = _statusRepository.GetAll();
            Assert.AreEqual(8, result.Count());
            Assert.AreEqual("Test 2", result.ElementAt(result.Count() - 2).Description);
            Assert.AreEqual("Test" , result.ElementAt(result.Count() - 1).Description);
        }

        [Test]
        public void Update_ModifiesExistingStatus()
        {
            var status = _statusRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            status.Description = "Updated";
            _statusRepository.Update(status);
            _context.SaveChanges();

            var result = _statusRepository.GetAll();
            Assert.AreEqual("Updated", result.First().Description);
        }

        [Test]
        public void Delete_RemovesStatus()
        {
            var status = _statusRepository.Get(s => s.Id == new Guid("a57b5870-874a-4bcd-8cc1-09fe75a817ce"));
            _statusRepository.Remove(status);
            _context.SaveChanges();

            var result = _statusRepository.GetAll();
            Assert.AreEqual(5, result.Count());
            Assert.AreEqual("In-Progress", result.First().Description);
        }

        [Test]
        public void Delete_RemoveRangeStatuses()
        {
            var status = _statusRepository.Find(s => s.Description == "Testing");
            _statusRepository.RemoveRange(status);
            _context.SaveChanges();

            var result = _statusRepository.GetAll();
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("Resolved", result.Last().Description);
        }

    }
}
