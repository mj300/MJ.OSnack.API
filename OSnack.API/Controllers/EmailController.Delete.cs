namespace OSnack.API.Controllers
{
   public partial class EmailController
   {
      //#region *** ***
      //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      //[ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      //[ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      //#endregion
      //[HttpDelete("[action]/{emailTemplateId}")]
      //[Authorize(AppConst.AccessPolicies.Secret)] /// Done
      //public async Task<IActionResult> DeleteTemplate(int emailTemplateId)
      //{
      //   try
      //   {
      //      EmailTemplate foundTemplate = await _DbContext.EmailTemplates.AsTracking()
      //         .FirstOrDefaultAsync((et) => et.Id == emailTemplateId)
      //         .ConfigureAwait(false);
      //      if (foundTemplate is null)
      //      {
      //         ErrorsList.Add(new Error("", "Template cannot be found."));
      //         return UnprocessableEntity(ErrorsList);
      //      }

      //      foundTemplate.DeleteFiles(WebHost.WebRootPath);

      //      _DbContext.Remove(foundTemplate);

      //      await _DbContext.SaveChangesAsync().ConfigureAwait(false);
      //      return Ok($"Email Template was deleted");
      //   }
      //   catch (Exception ex)
      //   {
      //      CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
      //      return StatusCode(417, ErrorsList);
      //   }
      //}
   }
}
