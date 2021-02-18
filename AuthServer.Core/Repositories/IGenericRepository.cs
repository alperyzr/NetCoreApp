using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Repositories
{
    public interface IGenericRepository<TEntity>where TEntity:class
    {
        Task<TEntity> GetByIdAsync(int id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        //where(x=> x.id > 5) x = TEntity
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity); //Create

        void Remove(TEntity entity);

        TEntity Update(TEntity entity);
    }
}
