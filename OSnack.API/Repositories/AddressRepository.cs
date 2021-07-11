using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class AddressRepository : Repository<Address>, IAddressRepository
   {
      public AddressRepository(OSnackDbContext context)
          : base(context)
      {
      }

      public async Task<Address> GetAddressByUserId(int addressId, int userId)
      {
         return await _DbContext.Addresses.Include(a => a.User)
              .SingleOrDefaultAsync(a => a.Id == addressId && a.User.Id == userId).ConfigureAwait(false);
      }

      public async Task<List<Address>> GetAllAddressByUserId(int userId)
      {
         return await _DbContext.Addresses.Include(a => a.User)
                    .ThenInclude(u => u.Role)
                    .Include(a => a.User)
                    .ThenInclude(u => u.RegistrationMethod)
                    .Where(a => a.User.Id == userId).ToListAsync();
      }
   }
}
