using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSnack.API.Database.Models;
using OSnack.API.Extras;
using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class DeliveryOptionController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpDelete("[action]/{deliveyOptionId}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      /// Ready For Test
      public async Task<IActionResult> Delete(int deliveyOptionId)
      {
         try
         {
            DeliveryOption currentDeliveryOption = await _DbContext.DeliveryOptions.SingleOrDefaultAsync(d => d.Id == deliveyOptionId)
                           .ConfigureAwait(false);
            if (currentDeliveryOption is null)
            {
               CoreFunc.Error(ref ErrorsList, "Delivery Option not found");
               return NotFound(ErrorsList);
            }

            if (currentDeliveryOption.IsPremitive)
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Delivery Option is primitive.");
               return StatusCode(412, ErrorsList);
            }

            _DbContext.DeliveryOptions.Remove(currentDeliveryOption);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok("Delivery Option was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
