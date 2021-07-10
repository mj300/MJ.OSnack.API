using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using OSnack.API.Database;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.JsonConvertor;
using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class MaintenanceController
   {

      #region *** ***
      [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(bool), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done  
      public async Task<IActionResult> Put([FromBody] bool status)
      {
         try
         {
            Settings settings = new Settings();

            string settingsPath = AppFunc.GetFilePath(@"StaticFiles\Settings.json");
            if (string.IsNullOrEmpty(settingsPath))
            {
               ErrorsList.Add(new Error("0", "Unable to change maintenance mode"));
               /// return Unprocessable Entity with all the errors
               return UnprocessableEntity(ErrorsList);
            }

            JsonConvert.PopulateObject(System.IO.File.ReadAllText(settingsPath), settings);

            settings.MaintenanceModeStatus = status;
            AppConst.Settings.MaintenanceModeStatus = status;

            await System.IO.File.WriteAllTextAsync(settingsPath, JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings
            {
               Converters = new List<JsonConverter> { new StringEnumConverter(), new DecimalFormatConverter() },
               ContractResolver = new DynamicContractResolver("OpenCors", "AntiforgeryCookieDomain", "ClientApp", "AdminApp", "MailServer"
               , "Sender", "Password", "AdminEmail", "PayPal", "ExternalLoginSecrets", "DbConnectionString", "GooglereCAPTCHASecret")
            }))
               .ConfigureAwait(false);

            AppConst.SetSettings();
            return Ok(status);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
