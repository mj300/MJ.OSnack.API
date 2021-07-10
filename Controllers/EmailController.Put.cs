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
   public partial class EmailController
   {
      #region *** ***
      [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done  
      public async Task<IActionResult> PutTemplate([FromBody] EmailTemplate emailTemplate)
      {
         try
         {
            /// if model validation failed
            if (!TryValidateModel(emailTemplate))
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               /// return Unprocessable Entity with all the errors
               return UnprocessableEntity(ErrorsList);
            }


            EmailTemplate foundTemplate = await _DbContext.EmailTemplates
               .FirstOrDefaultAsync((et) => et.Id == emailTemplate.Id)
               .ConfigureAwait(false);

            if (foundTemplate == null)
            {
               ErrorsList.Add(new Error("", "Template cannot be found."));
               /// return Unprocessable Entity with all the errors
               return UnprocessableEntity(ErrorsList);
            }

            foundTemplate.PrepareHtml(WebHost.WebRootPath);
            emailTemplate.RemoveHtmlComment();
            if (foundTemplate.HTML != emailTemplate.HTML)
            {
               emailTemplate.SaveFilesToWWWRoot(WebHost.WebRootPath);
               foundTemplate.DeleteFiles(WebHost.WebRootPath);
            }
            else
            {
               emailTemplate.DesignPath = foundTemplate.DesignPath;
               emailTemplate.HtmlPath = foundTemplate.HtmlPath;
            }
            _DbContext.EmailTemplates.Update(emailTemplate);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);
            return Ok(emailTemplate);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
