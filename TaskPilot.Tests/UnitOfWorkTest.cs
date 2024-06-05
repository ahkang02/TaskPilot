using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Data;
using TaskPilot.Infrastructure.Repository;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class UnitOfWorkTest
    {
        private TaskContext _context;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskContext(options);
            _unitOfWork = new UnitOfWork(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _unitOfWork.Dispose();
        }

        [Test]
        public void Repository_ReturnsCorrectRepository()
        {
            
        }

    }
}
