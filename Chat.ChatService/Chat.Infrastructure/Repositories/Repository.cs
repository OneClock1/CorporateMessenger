using Chat.Domain.Abstractions;
using Chat.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Infrastructure.Repositories
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _entities = _dbContext.Set<TEntity>();
        }

        private readonly DbSet<TEntity> _entities;

        private readonly DbContext _dbContext;

        public async Task<TEntity> CreateAsync(TEntity item)
        {
            var entity = await _entities.AddAsync(item);
            return entity.Entity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="includeProperties">Include properties separate with comma</param>
        /// <returns></returns>
        public Task<TEntity> FindByKeyAsync(TKey key, string includeProperties = null)
        {

            IQueryable<TEntity> query = _entities;

            query.AsNoTracking();

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query.FirstOrDefaultAsync(p => p.Id.Equals(key));
        }

        public Task<TEntity[]> GetAsync(Expression<Func<TEntity, bool>> expression = null,
                                        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,  
                                        string includeProperties = null)
        {

            IQueryable<TEntity> query = _entities;

            query.AsNoTracking();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).ToArrayAsync();
            }
            else
            {
                return query.ToArrayAsync();
            }
        }

        public TEntity Update(TEntity item)
        {
            return _entities.Update(item).Entity;
        }

        public bool Remove(TEntity item)
        {
            var removed = _entities.Remove(item);
            if (removed == null)
                return false;
            return true;
            
        }

        public bool Remove(TKey key)
        {
            var item = _entities.FirstOrDefault(p => p.Id.Equals(key));
            if (item == null)
                return false;

            var removed = _entities.Remove(item);
            if (removed == null)
                return false;
            return true;
            
        }
    }
}
