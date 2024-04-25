using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagementApp.DAL
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();

        TEntity GetById(object id);

        void Insert(TEntity obj);

        void Update(TEntity obj);

        void Delete(TEntity obj);

        void Save();
    }
}
