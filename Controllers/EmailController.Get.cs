using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class EmailController
   {
      #region *** ***        
      [ProducesResponseType(typeof(List<EmailTemplate>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done
      public async Task<IActionResult> AllTemplate()
      {
         try
         {
            List<EmailTemplate> templateList = await _DbContext.EmailTemplates
               .OrderByDescending(et => et.TemplateType)
               .ToListAsync()
               .ConfigureAwait(false);

            EmailTemplate copyDefaultTemplate = null;
            foreach (EmailTemplate item in templateList)
            {
               if (item.TemplateType == EmailTemplateTypes.DefaultTemplate)
                  copyDefaultTemplate = item;
               item.PrepareDesign(WebHost.WebRootPath);
               item.PrepareHtml(WebHost.WebRootPath);
            }

            if (copyDefaultTemplate != null)
            {
               templateList.Remove(copyDefaultTemplate);
               templateList = templateList.Prepend(copyDefaultTemplate).ToList();
            }
            return Ok(templateList);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User)); ;
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***                 
      [MultiResultPropertyNames("emailTemplate", "defaultEmailTemplate")]
      [ProducesResponseType(typeof(MultiResult<EmailTemplate, EmailTemplate>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("[action]/{templateId}")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done
      public async Task<IActionResult> GetTemplate(int templateId)
      {
         try
         {
            var defaultTemplate = await _DbContext.EmailTemplates
               .SingleOrDefaultAsync(et => et.TemplateType.Equals(EmailTemplateTypes.DefaultTemplate)).ConfigureAwait(false);
            var template = await _DbContext.EmailTemplates
               .SingleOrDefaultAsync(et => et.Id == templateId).ConfigureAwait(false);

            defaultTemplate.PrepareDesign(WebHost.WebRootPath);
            template.PrepareDesign(WebHost.WebRootPath);

            template.SetServerClasses();

            return Ok(new MultiResult<EmailTemplate, EmailTemplate>
               (template, defaultTemplate, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***                                                    
      [ProducesResponseType(typeof(List<EmailTemplateTypes>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public IActionResult GetAllAvailableTemplateTypes()
      {
         try
         {
            var typeList = Enum.GetValues(typeof(EmailTemplateTypes)).Cast<EmailTemplateTypes>().ToList();
            foreach (var item in _DbContext.EmailTemplates)
            {
               typeList.RemoveAll(t => t == item.TemplateType);
            }

            return Ok(typeList);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
