using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Domain.Abstractions
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        Task<TEntity> FindByKeyAsync(TKey key, string includeProperties = null);

        Task<TEntity[]> GetAsync(Expression<Func<TEntity, bool>> expression = null,
                                 Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                 string includeProperties = null);

        Task<TEntity> CreateAsync(TEntity item);

        TEntity Update(TEntity item);

        bool Remove(TEntity item);

        bool Remove(TKey key);
    }
}
