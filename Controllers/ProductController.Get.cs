using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Extentions;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class ProductController
   {
      #region ***  ***                
      [MultiResultPropertyNames("productList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<Product>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("GET/[action]/{selectedPage}/{maxItemsPerPage}/{filterCategory}/{searchValue}/{isSortAsce}/{sortName}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> SearchPublic(
          int selectedPage, int maxItemsPerPage,
          string filterCategory, string searchValue,
          bool isSortAsce, string sortName) =>
         await Search(selectedPage, maxItemsPerPage, filterCategory, searchValue, "true", isSortAsce, sortName).ConfigureAwait(false);

      #region ***  ***
      [MultiResultPropertyNames("productList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<Product>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("GET/[action]/{selectedPage}/{maxItemsPerPage}/{filterCategory}/{searchValue}/{filterStatus}/{isSortAsce}/{sortName}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> SearchSecret(
          int selectedPage, int maxItemsPerPage,
          string filterCategory, string searchValue,
          string filterStatus, bool isSortAsce, string sortName) =>
         await Search(selectedPage, maxItemsPerPage, filterCategory, searchValue, filterStatus, isSortAsce, sortName).ConfigureAwait(false);

      private async Task<IActionResult> Search(
          int selectedPage,
          int maxItemsPerPage,
          string filterCategory = CoreConst.GetAllRecords,
          string searchValue = CoreConst.GetAllRecords,
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = true,
          string sortName = "Name")
      {
         try
         {
            _ = bool.TryParse(filterStatus, out bool boolFilterStatus);
            _ = int.TryParse(filterCategory, out int filterProductCategoryId);

            int totalCount = await _DbContext.Products
                .Where(p => filterStatus.Equals(CoreConst.GetAllRecords) || p.Status == boolFilterStatus)
                .Where(p => filterCategory.Equals(CoreConst.GetAllRecords) || p.Category.Id == filterProductCategoryId)
                .CountAsync(p => searchValue.Equals(CoreConst.GetAllRecords) || (p.Name.Contains(searchValue) || p.Id.ToString().Contains(searchValue)))
                .ConfigureAwait(false);

            /// Include the necessary properties
            List<Product> list = await _DbContext.Products
                .Where(p => filterStatus.Equals(CoreConst.GetAllRecords) || p.Status == boolFilterStatus)
                .Include(p => p.Category)
                .Include(p => p.NutritionalInfo)
                .Include(p => p.Comments)
                .Where(p => filterCategory.Equals(CoreConst.GetAllRecords) || p.Category.Id == filterProductCategoryId)
                .Where(p => searchValue.Equals(CoreConst.GetAllRecords) || (p.Name.Contains(searchValue) || p.Id.ToString().Contains(searchValue)))
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxItemsPerPage)
                .Take(maxItemsPerPage)
                .ToListAsync()
                .ConfigureAwait(false);
            return Ok(new MultiResult<List<Product>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


      /// <summary>
      /// Get a single product by using product and category name 
      /// </summary>
      #region ***  ***         
      [MultiResultPropertyNames("product", "relatedProductList")]
      [ProducesResponseType(typeof(MultiResult<Product, List<Product>>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      #endregion
      [HttpGet("GET/[action]/{categoryName}/{productName}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> ProductAndRelate(string categoryName, string productName)
      {
         try
         {
            Product product = await _DbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .Include(p => p.NutritionalInfo)
                .SingleOrDefaultAsync(p => p.Category.Name.Equals(categoryName)
                                        && p.Name.Equals(productName)
                                        && p.Status)
                .ConfigureAwait(false);

            if (product == null)
            {
               CoreFunc.Error(ref ErrorsList, "Product Not Found");
               return NotFound(ErrorsList);
            }
            List<Product> relatedProducts = await _DbContext.Products
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


            return Ok(new MultiResult<Product, List<Product>>(product, relatedProducts, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
