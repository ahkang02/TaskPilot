using System.Linq.Expressions;

namespace TaskPilot.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAllInclude(Expression<Func<T, bool>>? filter, string? includeProperties = null);

        IEnumerable<T> GetAll();

        T Get(Expression<Func<T, bool>> filter);

        IEnumerable<T> Find(Expression<Func<T, bool>> filter);

        void Add(T entity);

        void AddRange(IEnumerable<T> entities);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

        void Update(T entity);
    }
}
