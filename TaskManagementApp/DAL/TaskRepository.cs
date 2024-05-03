using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class TaskRepository : IRepository<Tasks>, IDisposable
    {
        private readonly TaskContext _context;

        public TaskRepository(TaskContext context)
        {
            this._context = context;
        }

        public IEnumerable<Tasks> GetAll()
        {
            return _context.Tasks.ToList();
        }

        public IEnumerable<Tasks> GetAllInclude(Expression<Func<Tasks, bool>> filter = null, Func<IQueryable<Tasks>, IOrderedQueryable<Tasks>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Tasks> query = _context.Tasks.AsQueryable();

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

        public Tasks GetById(object id)
        {
            return _context.Tasks.Find(id);
        }

        public Tasks GetById(Guid id)
        {
            return _context.Tasks.Find(id);
        }


        public IEnumerable<Tasks> GetByTaskName(string taskName)
        {
            return _context.Tasks.Where(t => t.Name == taskName).ToList();
        }

        public void Insert(Tasks obj)
        {
            _context.Tasks.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Tasks obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(Tasks obj)
        {
            Tasks tasks = _context.Tasks.Find(obj.Id);
            _context.Tasks.Remove(tasks);
        }

        public void DeleteById(Guid Id)
        {
            Tasks tasks = _context.Tasks.Single(t => t.Id == Id);
            _context.Tasks.Remove(tasks);
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