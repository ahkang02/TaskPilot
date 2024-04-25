using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class PrioritiesRepository : IRepository<Priorities>, IDisposable
    {
        private TaskContext _context;

        public PrioritiesRepository(TaskContext context)
        {
            this._context = context;
        }

        public IEnumerable<Priorities> GetAll()
        {
            return _context.Priorities.ToList();
        }

        public Priorities GetById(object id)
        {
            return _context.Priorities.Find(id);
        }

        public Priorities GetByName(string description)
        {
            return _context.Priorities.SingleOrDefault(p => p.Description == description);
        }

        public void Insert(Priorities obj)
        {
            _context.Priorities.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Priorities obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(Priorities obj)
        {
            Priorities priority = _context.Priorities.Find(obj.Id);
            _context.Priorities.Remove(priority);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}