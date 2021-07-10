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
   public partial class CommunicationController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Communication), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test
      [HttpPut("Put/[action]/{communicationId}")]
      public async Task<IActionResult> PutOfficial([FromBody] Message message, string communicationId)
         => await Update(communicationId, true, message);

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Communication), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test
      [HttpPut("Put/[action]/{communicationId}/{status}")]
      public async Task<IActionResult> PutSecret([FromBody] Message message, string communicationId, bool status)
         => await Update(communicationId, false, message, status);

      private async Task<IActionResult> Update(string communicationId, bool isCustomer, Message message, bool status = false)
      {
         try
         {
            var originalCommunication = await _DbContext.Communications
               .Include(c => c.Messages)
               .SingleOrDefaultAsync(c => c.Id == communicationId);

            if (originalCommunication is null)
            {
               CoreFunc.Error(ref ErrorsList, "Communication Not exists.");
               return StatusCode(412, ErrorsList);
            }

            if (!isCustomer)
               originalCommunication.Status = status;


            if (string.IsNullOrWhiteSpace(message.Body) && originalCommunication.Status)
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Message is required.");
               return StatusCode(412, ErrorsList);
            }

            if (!originalCommunication.Status)
               message.Body = $"{originalCommunication.CommunicationType} was closed";

            message.IsCustomer = isCustomer;
            originalCommunication.Messages.Add(message);

            ModelState.Clear();
            TryValidateModel(originalCommunication);

            foreach (var key in ModelState.Keys)
            {
               if (key.StartsWith("User") || key.StartsWith("Order") || key.StartsWith("OrderItem"))
                  ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            _DbContext.Communications.Update(originalCommunication);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            if (message.IsCustomer)
               await _EmailService.MessageToAdminAsync(message, originalCommunication).ConfigureAwait(false);
            else if (originalCommunication.Status)
               await _EmailService.MessageToUser(message, originalCommunication).ConfigureAwait(false);

            return Ok(originalCommunication);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
