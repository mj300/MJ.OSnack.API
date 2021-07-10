using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using OSnack.API.Database;
using OSnack.API.Extras;

using P8B.Core.CSharp;
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
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done  
      public IActionResult Get()
      {
         try
         {
            Settings settings = new Settings();

            string settingsPath = AppFunc.GetFilePath(@"StaticFiles\Settings.json");

            return Ok(AppConst.Settings.MaintenanceModeStatus);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
