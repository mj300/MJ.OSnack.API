using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class AuthenticationController
   {

      [ProducesResponseType(typeof(string), StatusCodes.Status418ImATeapot)]
      [HttpGet("Get/[action]")]
      public IActionResult DatabaseConnectionFailed() =>
          StatusCode(418, "Database Connection Failed");

      #region *** ***
      [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]
      [HttpGet("Get/[action]")]
      public async Task<IActionResult> Logout()
      {
         try
         {
            await _SignInManager.SignOutAsync().ConfigureAwait(false);
            SetAntiforgeryCookie();
            return Ok();
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region ***  ***

      [MultiResultPropertyNames("user", "isAuthenticated", "maintenanceModeStatus", "isUserAllowedInMaintenance")]
      [ProducesResponseType(typeof(MultiResult<User, bool, bool, bool>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Public)]
      [HttpGet("Get/[action]")]
      public async Task<IActionResult> Silence()
      {
         try
         {
            User user = new User();
            bool isAuthenticated = false;
            if (_SignInManager.IsSignedIn(User))
               foreach (string policy in AppFunc.GetCurrentRequestPolicies(Request))
               {
                  AuthorizationResult authResult = await _AuthService.AuthorizeAsync(User, policy).ConfigureAwait(false);
                  if (authResult.Succeeded)
                  {
                     user = await _DbContext.Users
                        .Include(u => u.Role)
                        .Include(u => u.RegistrationMethod)
                        .FirstOrDefaultAsync(u => u.Id == AppFunc.GetUserId(User))
                        .ConfigureAwait(false);
                     isAuthenticated = true;
                     break;
                  }
               }
            SetAntiforgeryCookie();

            bool maintenanceModeStatus = AppConst.Settings.MaintenanceModeStatus;
            bool isUserAllowedInMaintenance = false;
            if (user.Role != null
              && (user.Role.AccessClaim == AppConst.AccessClaims.Admin
               || user.Role.AccessClaim == AppConst.AccessClaims.Manager))
               isUserAllowedInMaintenance = true;
            Request.Headers.TryGetValue("Origin", out StringValues OriginValue);
            if (maintenanceModeStatus && AppConst.Settings.AppDomains.AdminApp.EqualCurrentCultureIgnoreCase(OriginValue))
               maintenanceModeStatus = false;
            return Ok(new MultiResult<User, bool, bool, bool>(user, isAuthenticated, maintenanceModeStatus, isUserAllowedInMaintenance
               , CoreFunc.GetCustomAttributeTypedArgument(ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      private void SetAntiforgeryCookie()
      {
         CookieOptions antiForgeryCookieOptions = new CookieOptions()
         {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Secure = true,
         };
         if (_WebHostingEnv.IsProduction())
            antiForgeryCookieOptions.Domain = AppConst.Settings.AppDomains.AntiforgeryCookieDomain;
         AntiforgeryTokenSet tokens = _Antiforgery.GetAndStoreTokens(HttpContext);

         Response.Cookies.Append(
            "AF-TOKEN",
            tokens.RequestToken,
            antiForgeryCookieOptions);
      }

   }
}
