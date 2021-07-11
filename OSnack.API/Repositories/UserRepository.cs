using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using P8B.Core.CSharp.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class UserRepository : Repository<User>, IUserRepository
   {
      public UserRepository(OSnackDbContext context)
            : base(context)
      {
      }

      public async Task<User> GetUser(int userId)
      {
         return await _DbContext.Users
                   .Include(u => u.Role)
                   .Include(u => u.RegistrationMethod)
                   .SingleOrDefaultAsync(u => u.Id == userId);
      }

      public async Task<User> GetUser(Expression<Func<User, bool>> predicate)
      {
         return await _DbContext.Users.Include(u => u.Role)
                 .Include(u => u.RegistrationMethod)
                  .SingleOrDefaultAsync(predicate);
      }

      public async Task<List<User>> GetUsersByPageAsync(Expression<Func<User, bool>> predicate, bool isSortAsce, string sortName, int selectedPage, int maxNumberPerItemsPage)
      {
         return await _DbContext.Users
                .Include(u => u.Role)
                .Include(u => u.RegistrationMethod)
                .Where(predicate)
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .Include(u => u.Orders)
                .ToListAsync()
                .ConfigureAwait(false);
      }

      public async Task<User> GetUserWithItsOrder(int userId)
      {
         return await _DbContext.Users
               .Include(u => u.Orders)
               .ThenInclude(o => o.OrderItems)
               .SingleOrDefaultAsync(u => u.Id == userId)
               .ConfigureAwait(false);
      }

      public async Task<int> UserCountAsync(Expression<Func<User, bool>> predicate)
      {
         return await _DbContext.Users
                 .Include(u => u.Role)
                 .CountAsync(predicate)
                 .ConfigureAwait(false);
      }
   }
}
