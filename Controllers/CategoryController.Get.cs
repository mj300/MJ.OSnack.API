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
   public partial class CategoryController
   {
      /// <summary>
      /// Search or get all the categories.
      /// search by name or filter by unit or status
      /// </summary>
      #region *** ***
      [MultiResultPropertyNames("categoryList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<Category>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{selectedPage}/{maxNumberPerItemsPage}/{searchValue}/{isSortAsce}/{sortName}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> Search(
          int selectedPage,
          int maxNumberPerItemsPage,
          string searchValue = "",
          bool isSortAsce = true,
          string sortName = "Name")
      {
         try
         {
            int totalCount = await _DbContext.Categories
                .CountAsync(c => searchValue.Equals(CoreConst.GetAllRecords) ? true : c.Name.Contains(searchValue))
                .ConfigureAwait(false);

            List<Category> list = await _DbContext.Categories
                .Where(c => searchValue.Equals(CoreConst.GetAllRecords) ? true : c.Name.Contains(searchValue))
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var category in list)
            {
               category.TotalProducts = await _DbContext.Products
                  .CountAsync(p => p.Category.Id == category.Id)
                  .ConfigureAwait(false);
            }
            return Ok(new MultiResult<List<Category>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region ***  ***
      [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> AllPublic()
      {
         try
         {
            return Ok(await _DbContext.Categories.Include(c => c.Products)
                                                 .Where(c => c.Products.Count(p => p.Status) > 0)
                                                 .ToListAsync().ConfigureAwait(false));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
      #region ***  ***
      [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> AllSecret()
      {
         try
         {
            return Ok(await _DbContext.Categories.ToListAsync().ConfigureAwait(false));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


   }
}
