using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class FeaturesRepository : IRepository<Features>, IDisposable
    {
        private TaskContext _context;
        public FeaturesRepository(TaskContext context)
        {
            this._context = context;
        }

        public IEnumerable<Features> GetAll()
        {
            return _context.Features.ToList();
        }

        public IEnumerable<Features> GetAllInclude(Expression<Func<Features, bool>> filter = null, Func<IQueryable<Features>, IOrderedQueryable<Features>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Features> query = _context.Features.AsQueryable();

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

        public Features GetById(object id)
        {
            return _context.Features.Find(id);
        }

        public Features GetById(Guid id)
        {
            return _context.Features.Find(id);
        }


        public IEnumerable<Features> GetByTaskName(string taskName)
        {
            return _context.Features.Where(t => t.Name == taskName).ToList();
        }

        public void Insert(Features obj)
        {
            _context.Features.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Features obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(Features obj)
        {
            Features tasks = _context.Features.Find(obj.Id);
            _context.Features.Remove(tasks);
        }

        public void DeleteById(Guid Id)
        {
            Features tasks = _context.Features.Single(t => t.Id == Id);
            _context.Features.Remove(tasks);
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
