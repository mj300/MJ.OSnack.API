using OSnack.API.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IAddressRepository : IRepository<Address>
   {
      Task<List<Address>> GetAllAddressByUserId(int userId);
      Task<Address> GetAddressByUserId(int addressId, int userId);
   }
}
