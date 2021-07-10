using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
   public partial class NewsletterController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpDelete("[action]/{key}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> Delete(string key)
      {
         try
         {
            /// if the Newsletter record with the same id is not found
            Newsletter newsletter = _DbContext.Newsletters.Find(key);
            if (newsletter != null)
            {
               /// now delete the Newsletter record
               _DbContext.Newsletters.Remove(newsletter);

               /// save the changes to the database
               await _DbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
               CoreFunc.Error(ref ErrorsList, "Your key is invalid.");
               return StatusCode(412, ErrorsList);
            }
            /// return 200 OK status
            return Ok($"{newsletter.Email} is now unsubscribed");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
