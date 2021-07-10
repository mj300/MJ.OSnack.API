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
using System.Threading.Tasks;
namespace OSnack.API.Controllers
{
   public partial class UserController
   {
      #region *** ***
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpDelete("[action]/{userId}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> Delete(int userId)
      {
         try
         {
            User user = await _DbContext.Users.SingleOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
            if (user is null)
            {
               CoreFunc.Error(ref ErrorsList, "User not found");
               return StatusCode(412, ErrorsList);
            }

            _DbContext.Users.Remove(user);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok($"User ID ('{user.Id}') was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
