using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IRepository<TEntity> where TEntity : class
   {
      TEntity Get(int id);
      Task<List<TEntity>> GetAllAsync();
      Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
      Task<TEntity> FindAsync(params object[] keyValues);
      Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
      Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
      Task<List<TEntity>> GetByPageAsync(Expression<Func<TEntity, bool>> predicate, bool isSortAsce,
          string sortName, int selectedPage,
          int maxNumberPerItemsPage);
      Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
      Task AddAsync(TEntity entity);
      void Modify(TEntity entity);
      void ModifyRange(IEnumerable<TEntity> entities);
      void AddRange(IEnumerable<TEntity> entities);
      void Remove(TEntity entity);
      void RemoveRange(IEnumerable<TEntity> entities);
   }
}
