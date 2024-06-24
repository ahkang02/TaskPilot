using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly TaskContext _context;
        internal DbSet<T> dbSet;

        public Repository(TaskContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                dbSet.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error in Add method: {ex.Message}");
                throw;
            }
        }

        public void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                dbSet.AddRange(entities);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error in AddRange method: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filter)
        {
            try
            {
                return dbSet.Where(filter).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Find method: {ex.Message}");
                throw;
            }
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            try
            {
                return dbSet.Where(filter).FirstOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationExeception in Get method: {ex.Message}");
                throw new ArgumentNullException("Entity not found.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Get method: {ex.Message}");
                throw;
            }

        }

        public IEnumerable<T> GetAllInclude(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            try
            {
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (!string.IsNullOrEmpty(includeProperties))
                {
                    foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(property);
                    }
                }
                return query.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllInclude method: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                return dbSet.ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll method: {ex.Message}");
                throw;
            }
        }


        public void Remove(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                dbSet.Remove(entity);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error in Remove method: {ex.Message}");
                throw;
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            try
            {
                dbSet.RemoveRange(entities);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error in RemoveRange method: {ex.Message}");
                throw;
            }
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                dbSet.Update(entity);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error in Update method: {ex.Message}");
                throw;
            }
        }
    }
}
