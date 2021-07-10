using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class EmailController
   {
      #region *** ***
      [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done   
      public async Task<IActionResult> PostTemplate([FromBody] EmailTemplate emailTemplate)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            /// save files
            emailTemplate.SaveFilesToWWWRoot(WebHost.WebRootPath);

            _DbContext.EmailTemplates.Add(emailTemplate);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Created("", emailTemplate);
         }
         catch (Exception ex)
         {
            emailTemplate.DeleteFiles(WebHost.WebRootPath);
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
