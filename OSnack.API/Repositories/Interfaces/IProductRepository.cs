using OSnack.API.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IProductRepository : IRepository<Product>
   {

      Task AddProductAsync(Product product);

      Task<Product> GetProductByName(string categoryName, string productName);
      Task<List<Product>> GetRelateProduct(Product product);

      Task<List<Product>> GetAllProductIncludeCategory(Expression<Func<Product, bool>> predicate);

      Task<List<Product>> GetProductByPageAsync(Expression<Func<Product, bool>> predicate, bool isSortAsce,
  string sortName, int selectedPage,
  int maxNumberPerItemsPage);

   }
}
