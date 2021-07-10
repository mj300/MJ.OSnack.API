using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class CouponController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Coupon), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(System.Collections.Generic.List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status422UnprocessableEntity)]
      #endregion
      [HttpPost("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      /// Ready For Test
      public async Task<IActionResult> Post([FromBody] Coupon newCoupon)
      {
         try
         {

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            if (!newCoupon.IsValid(ref ErrorsList))
            {
               return UnprocessableEntity(ErrorsList);
            }
            if (await _DbContext.Coupons.AnyAsync(c => c.Code == newCoupon.Code)
               .ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Coupon Code already exists.");
               return StatusCode(412, ErrorsList);
            }

            await _DbContext.Coupons.AddAsync(newCoupon).ConfigureAwait(false);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Created("Success", newCoupon);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
