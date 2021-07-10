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
   public partial class RoleController
   {
      #region ***  ***
      [ProducesResponseType(typeof(List<Role>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Done  
      public async Task<IActionResult> Get()
      {
         try
         {
            return Ok(await _DbContext.Roles.ToListAsync().ConfigureAwait(false));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
