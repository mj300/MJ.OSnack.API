using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Repositories.Interfaces;
using P8B.Core.CSharp.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
   {
      protected readonly OSnackDbContext _DbContext;

      public Repository(OSnackDbContext context) => _DbContext = context;

      public TEntity Get(int id) => _DbContext.Set<TEntity>().Find(id);

      public async Task<List<TEntity>> GetAllAsync() => await _DbContext.Set<TEntity>().ToListAsync();


      public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
         => await _DbContext.Set<TEntity>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);

      public async Task AddAsync(TEntity entity) => await _DbContext.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);

      public void Modify(TEntity entity)
      {
         _DbContext.Entry(entity).State = EntityState.Modified;
      }

      public void ModifyRange(IEnumerable<TEntity> entities)
      {
         foreach (var entity in entities)
         {
            _DbContext.Entry(entity).State = EntityState.Modified;
         }
      }

      public void AddRange(IEnumerable<TEntity> entities) => _DbContext.Set<TEntity>().AddRange(entities);


      public void Remove(TEntity entity) => _DbContext.Set<TEntity>().Remove(entity);

      public void RemoveRange(IEnumerable<TEntity> entities) => _DbContext.Set<TEntity>().RemoveRange(entities);

      public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
      {
         return await _DbContext.Set<TEntity>().CountAsync(predicate).ConfigureAwait(false);
      }

      public async Task<List<TEntity>> GetByPageAsync(Expression<Func<TEntity, bool>> predicate, bool isSortAsce, string sortName, int selectedPage, int maxNumberPerItemsPage)
      {
         return await _DbContext.Set<TEntity>().Where(predicate)
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .ToListAsync().ConfigureAwait(false);
      }

      public async Task<TEntity> FindAsync(params object[] keyValues)
      {
         return await _DbContext.Set<TEntity>().FindAsync(keyValues).ConfigureAwait(false);
      }

      public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
      {
         return await _DbContext.Set<TEntity>().AnyAsync(predicate).ConfigureAwait(false);
      }

      public async Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
      {
         return await _DbContext.Set<TEntity>().Where(predicate).ToListAsync().ConfigureAwait(false);
      }


   }
}
