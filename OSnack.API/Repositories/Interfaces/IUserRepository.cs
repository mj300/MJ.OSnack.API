using OSnack.API.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IUserRepository : IRepository<User>
   {
      Task<User> GetUser(int userId);
      Task<User> GetUserWithItsOrder(int userId);

      Task<User> GetUser(Expression<Func<User, bool>> predicate);

      Task<int> UserCountAsync(Expression<Func<User, bool>> predicate);

      Task<List<User>> GetUsersByPageAsync(Expression<Func<User, bool>> predicate, bool isSortAsce,
         string sortName, int selectedPage,
         int maxNumberPerItemsPage);
   }
}
