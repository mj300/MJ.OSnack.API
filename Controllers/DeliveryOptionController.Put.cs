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
      [ProducesResponseType(typeof(DeliveryOption), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Done   
      public async Task<IActionResult> Put([FromBody] DeliveryOption modifiedDeliveryOption)
      {
         try
         {
            /// get the current category
            DeliveryOption currentDeliveryOption = await _DbContext.DeliveryOptions
                .SingleOrDefaultAsync(c => c.Id == modifiedDeliveryOption.Id)
                .ConfigureAwait(false);

            // if the current category does not exists
            if (currentDeliveryOption == null)
            {
               CoreFunc.Error(ref ErrorsList, "Category Not Found");
               return NotFound(ErrorsList);
            }

            if (await _DbContext.DeliveryOptions
              .AnyAsync(d => d.Name.Equals(modifiedDeliveryOption.Name) && d.Id != modifiedDeliveryOption.Id).ConfigureAwait(false))
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Delivery Option Name already exists.");
               return StatusCode(412, ErrorsList);
            }

            currentDeliveryOption.Name = modifiedDeliveryOption.Name;
            if (!(currentDeliveryOption.IsPremitive && currentDeliveryOption.MinimumOrderTotal == 0))
               currentDeliveryOption.MinimumOrderTotal = modifiedDeliveryOption.MinimumOrderTotal;
            if (!(currentDeliveryOption.IsPremitive && currentDeliveryOption.Price == 0))
               currentDeliveryOption.Price = modifiedDeliveryOption.Price;


            TryValidateModel(currentDeliveryOption);

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            _DbContext.DeliveryOptions.Update(currentDeliveryOption);

            await _DbContext.SaveChangesAsync().ConfigureAwait(false);


            return Ok(currentDeliveryOption);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
