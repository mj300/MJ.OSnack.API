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
      #region *** 201 Created, 400 BadRequest, 422 UnprocessableEntity, 417 ExpectationFailed ***
      [HttpPost("[action]")]
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> Post([FromBody] Newsletter newsletter)
      {
         try
         {
            if (_DbContext.Newsletters.Find(newsletter.Email) != null)
               return Created("Success", "Thank you for your subscription.");

            TryValidateModel(newsletter);
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            await _DbContext.Newsletters.AddAsync(newsletter).ConfigureAwait(false);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            return Created("Success", "Thank you for your subscription.");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
