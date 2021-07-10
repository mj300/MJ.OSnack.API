using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;

using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class CouponController
   {

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpDelete("[action]/{couponCode}")]
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test
      public async Task<IActionResult> Delete(string couponCode)
      {
         try
         {
            Coupon coupon = await _DbContext.Coupons.SingleOrDefaultAsync(d => d.Code == couponCode)
                           .ConfigureAwait(false);
            if (coupon is null)
            {
               CoreFunc.Error(ref ErrorsList, "Coupon not found");
               return NotFound(ErrorsList);
            }

            _DbContext.Coupons.Remove(coupon);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok($"Coupon '{coupon.Code}' was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
