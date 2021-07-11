using OSnack.API.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{

   public interface ICategoryRepository : IRepository<Category>
   {
      Task<List<Category>> GetAvailableCategoryWithProduct();
   }
}
