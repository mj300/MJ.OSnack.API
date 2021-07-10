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
   public partial class DeliveryOptionController : ControllerBase
   {
      #region *** ***
      [ProducesResponseType(typeof(List<DeliveryOption>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]  /// Ready For Test  
      public async Task<IActionResult> All()
      {
         try
         {
            return Ok(await _DbContext.DeliveryOptions.ToListAsync());
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


      /// <summary>
      /// Search or get all the categories.
      /// search by name or filter by unit or status
      /// </summary>
      #region *** ***
      [MultiResultPropertyNames("deliveryOptionList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<DeliveryOption>, int>), StatusCodes.Status200OK)]
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
            int totalCount = await _DbContext.DeliveryOptions
                .CountAsync(c => searchValue.Equals(CoreConst.GetAllRecords) || c.Name.Contains(searchValue))
                .ConfigureAwait(false);

            List<DeliveryOption> list = await _DbContext.DeliveryOptions
                .Where(c => searchValue.Equals(CoreConst.GetAllRecords) ? true : c.Name.Contains(searchValue))
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(new MultiResult<List<DeliveryOption>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
