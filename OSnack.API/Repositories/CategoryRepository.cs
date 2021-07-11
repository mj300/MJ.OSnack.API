using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class CategoryRepository : Repository<Category>, ICategoryRepository
   {
      public CategoryRepository(OSnackDbContext context)
        : base(context)
      {
      }

      public async Task<List<Category>> GetAvailableCategoryWithProduct()
      {
         return await _DbContext.Categories.Include(c => c.Products)
                                                  .Where(c => c.Products.Any(p => p.Status))
                                                  .ToListAsync().ConfigureAwait(false);
      }
   }
}
