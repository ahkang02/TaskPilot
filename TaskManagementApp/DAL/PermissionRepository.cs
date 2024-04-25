using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class PermissionRepository : IRepository<Permission>, IDisposable
    {
        private TaskContext _context;

        public PermissionRepository(TaskContext context)
        {
            this._context = context;    
        }

        public IEnumerable<Permission> GetAll()
        {
            return _context.Permissions.ToList();
        }

        public IEnumerable<Permission> GetAllInclude(Expression<Func<Permission, bool>> filter = null, Func<IQueryable<Permission>, IOrderedQueryable<Permission>> orderBy = null, string includeProperties = "")
        {
            IQueryable<Permission> query = _context.Permissions.AsQueryable();

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
        public Permission GetById(object id)
        {
            return _context.Permissions.Find(id);
        }

        public Permission GetByName(string name)
        {
            return _context.Permissions.SingleOrDefault(p => p.Name == name);
        }

        public void Insert(Permission obj)
        {
            _context.Permissions.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Permission obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(Permission obj)
        {
            Permission permission = _context.Permissions.Find(obj.Id);
            _context.Permissions.Remove(permission);
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