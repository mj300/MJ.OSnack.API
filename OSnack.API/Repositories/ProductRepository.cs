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
   public class ProductRepository : Repository<Product>, IProductRepository
   {
      public ProductRepository(OSnackDbContext context)
          : base(context)
      {
      }

      public async Task AddProductAsync(Product product)
      {
         await _DbContext.Products.AddAsync(product).ConfigureAwait(false);
         _DbContext.Entry(product.Category).State = EntityState.Unchanged;
      }

      public async Task<List<Product>> GetAllProductIncludeCategory(Expression<Func<Product, bool>> predicate)
      {
         return await _DbContext.Products.AsTracking()
            .Include(p => p.Category)
            .Where(predicate)
            .ToListAsync()
            .ConfigureAwait(false);
      }

      public async Task<Product> GetProductByName(string categoryName, string productName)
      {
         return await _DbContext.Products
                 .Include(p => p.Category)
                 .Include(p => p.Comments)
                 .Include(p => p.NutritionalInfo)
                 .SingleOrDefaultAsync(p => p.Category.Name.Equals(categoryName)
                                         && p.Name.Equals(productName)
                                         && p.Status)
                 .ConfigureAwait(false);
      }

      public async Task<List<Product>> GetProductByPageAsync(Expression<Func<Product, bool>> predicate, bool isSortAsce, string sortName, int selectedPage, int maxNumberPerItemsPage)
      {
         return await _DbContext.Products
                 .Include(p => p.Category)
                 .Include(p => p.NutritionalInfo)
                 .Include(p => p.Comments)
                 .Where(predicate)
                 .OrderByDynamic(sortName, isSortAsce)
                 .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                 .Take(maxNumberPerItemsPage)
                 .ToListAsync()
                 .ConfigureAwait(false);
      }

      public async Task<List<Product>> GetRelatedProduct(Product product)
      {
         var relatedProducts = await _DbContext.Products
                 .Include(p => p.Category)
                 .Where(p => p.Category.Id == product.Category.Id
                          && p.Id != product.Id
                          && p.Status)
                 .Take(3)
                 .ToListAsync()
                 .ConfigureAwait(false);

         if (relatedProducts.Count < 3)
            relatedProducts.AddRange(await _DbContext.Products
             .Include(p => p.Category)
             .Take(3 - relatedProducts.Count)
             .Where(p => !relatedProducts.Contains(p)
                      && p.Id != product.Id
                      && p.Status)
             .ToListAsync()
             .ConfigureAwait(false));

         return relatedProducts;
      }

      public Task<List<Product>> GetRelateProduct(Product product)
      {
         throw new NotImplementedException();
      }
   }
}
