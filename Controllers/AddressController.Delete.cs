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
   public partial class AddressController
   {
      #region*** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpDelete("[action]/{addressId}")]
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test 
      public async Task<IActionResult> Delete(int addressId)
      {
         try
         {
            Address address = await _DbContext.Addresses.SingleAsync(a => a.Id == addressId).ConfigureAwait(false);
            if (address is null)
            {
               CoreFunc.Error(ref ErrorsList, "Address not found");
               return NotFound(ErrorsList);
            }

            if (address.IsDefault == true)
            {
               CoreFunc.Error(ref ErrorsList, "Unable to Delete default Address.");
               return StatusCode(412, ErrorsList);
            }

            _DbContext.Addresses.Remove(address);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok($"Address was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
