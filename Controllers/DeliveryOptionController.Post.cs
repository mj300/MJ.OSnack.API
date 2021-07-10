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
      [ProducesResponseType(typeof(DeliveryOption), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test
      [HttpPost("[action]")]
      public async Task<IActionResult> Post([FromBody] DeliveryOption newDeliveryOption)
      {
         try
         {
            /// check the database to see if a Category with the same name exists
            if (await _DbContext.DeliveryOptions
                .AnyAsync(d => d.Name.Equals(newDeliveryOption.Name)).ConfigureAwait(false))
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Delivery Option Name already exists.");
               return StatusCode(412, ErrorsList);
            }

            TryValidateModel(newDeliveryOption);


            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            newDeliveryOption.IsPremitive = false;
            /// else Category object is made without any errors
            /// Add the new Category to the EF context
            await _DbContext.DeliveryOptions.AddAsync(newDeliveryOption).ConfigureAwait(false);
            /// save the changes to the database
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);


            /// return 201 created status with the new object
            /// and success message
            return Created("", newDeliveryOption);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
