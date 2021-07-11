using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class AddressController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Address), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test  
      [HttpPut("[action]")]
      public async Task<IActionResult> Put([FromBody] Address modifiedAddress)
      {
         try
         {
            if (modifiedAddress != null)
            {
               modifiedAddress.User = await _unitOfWork.Users.GetUser(AppFunc.GetUserId(User));
            }
            ModelState.Clear();
            if (!TryValidateModel(modifiedAddress))
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            _unitOfWork.Addresses.Modify(modifiedAddress);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return Ok(modifiedAddress);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("Put/[action]")]
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test 
      public async Task<IActionResult> SetDefault([FromBody] int addressId)
      {
         try
         {
            List<Address> addresses = await _unitOfWork.Addresses.GetAllAddressByUserId(AppFunc.GetUserId(User));
            if (addresses.SingleOrDefault(a => a.Id == addressId) != null)
            {
               foreach (var address in addresses)
               {
                  if (address.Id == addressId)
                     address.IsDefault = true;
                  else
                     address.IsDefault = false;
               }
            }

            _unitOfWork.Addresses.ModifyRange(addresses);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return NoContent();
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
