using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class NotificationRepository : IRepository<Notifications>, IDisposable
    {
        private readonly TaskContext _context;

        public NotificationRepository(TaskContext context)
        {
            this._context = context;
        }

        public IEnumerable<Notifications> GetAll()
        {
           return _context.Notifications.ToList();
        }

        public IEnumerable<Notifications> GetAllInclude(Expression<Func<Notifications, bool>> filter = null, Func<IQueryable<Notifications>, IOrderedQueryable<Notifications>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Notifications> query = _context.Notifications.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public Notifications GetById(object id)
        {
            return _context.Notifications.Find(id);
        }

        public Notifications GetById(Guid Id)
        {
            return _context.Notifications.Find(Id);
        }

        public Notifications GetByTaskId(Guid Id)
        {
            return _context.Notifications.SingleOrDefault(n => n.TasksId == Id);
        }

        public void Insert(Notifications obj)
        {
            _context.Notifications.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Notifications obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }
        public void Delete(Notifications obj)
        {
            Notifications notifications = _context.Notifications.Find(obj.Id);
            _context.Notifications.Remove(notifications);
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