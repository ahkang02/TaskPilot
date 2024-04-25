using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class StatusesRepository : IRepository<Statuses>, IDisposable
    {
        private TaskContext _context;

        public StatusesRepository(TaskContext context)
        {
            this._context = context;
        }

        public IEnumerable<Statuses> GetAll()
        {
            return _context.Statuses.ToList();
        }

        public Statuses GetById(object id)
        {
            return _context.Statuses.Find(id);
        }

        public Statuses GetByName(string name)
        {
            return _context.Statuses.SingleOrDefault(s => s.Description == name);
        }

        public void Insert(Statuses obj)
        {
            _context.Statuses.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Statuses obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(Statuses obj)
        {
            Statuses status = _context.Statuses.Find(obj.Id);
            _context.Statuses.Remove(status);
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