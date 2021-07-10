using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class AuthenticationController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(User), StatusCodes.Status206PartialContent)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status403Forbidden)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> ExternalLogin(ExternalLoginDetails externalLoginInfo)
      {
         try
         {

            IEnumerable<string> policies = AppFunc.GetCurrentRequestPolicies(Request, out AppTypes apptype);
            if (apptype == AppTypes.Invalid)
            {
               CoreFunc.Error(ref ErrorsList, "Unauthorised Application Access. 🤔");
               return Unauthorized(ErrorsList);
            }

            if (!TryValidateModel(externalLoginInfo))
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            User externalLoginUser = new User();
            switch (externalLoginInfo.Type)
            {
               case RegistrationTypes.Google:
                  externalLoginUser = await GetGoogleUserInfo(externalLoginInfo).ConfigureAwait(false);
                  break;
               case RegistrationTypes.Facebook:
                  externalLoginUser = await GetFacebookUserInfo(externalLoginInfo).ConfigureAwait(false);
                  break;
            }

            // Check if the user is already registered 
            User registeredUser = await _DbContext.Users
               .Include(u => u.Role)
               .Include(u => u.RegistrationMethod)
               .SingleOrDefaultAsync(u => u.RegistrationMethod.Type == externalLoginInfo.Type
               && u.RegistrationMethod.ExternalLinkedId == externalLoginUser.RegistrationMethod.ExternalLinkedId)
               .ConfigureAwait(false);

            // if the user is already registered
            if (registeredUser != null)
            {
               if (!await IsUserPolicyAccepted(registeredUser, policies).ConfigureAwait(false))
               {
                  CoreFunc.Error(ref ErrorsList, "You do not have permission to login here.");
                  return Unauthorized(ErrorsList);
               }

               // sign the user in without any password
               await _SignInManager.SignInAsync(registeredUser, externalLoginInfo.RememberMe).ConfigureAwait(false);
               return Ok(registeredUser);
            }

            if (apptype == AppTypes.Admin)
            {
               CoreFunc.Error(ref ErrorsList, "You do not have permission to login here. Please contact administrator.");
               return Unauthorized(ErrorsList);
            }
            /// check if the user is registered using other methods
            User user = await _UserManager
                    .FindByEmailAsync(externalLoginUser?.Email).ConfigureAwait(false);
            if (user != null)
            {
               RegistrationMethod registrationMethod = await _DbContext.RegistrationMethods
                  .FirstOrDefaultAsync(r => r.User.Id == user.Id).ConfigureAwait(false);

               if (registrationMethod.Type != RegistrationTypes.Application)
               {
                  /// in the case any exceptions return the following error
                  CoreFunc.Error(ref ErrorsList, $"Please use {registrationMethod.Type} account to login.");
               }
               else
               {
                  /// in the case any exceptions return the following error
                  CoreFunc.Error(ref ErrorsList, $"Please use Email and Password to login");
               }
               return StatusCode(403, ErrorsList);
            }

            externalLoginUser.Role = await _DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Customer").ConfigureAwait(false);
            // else if the user is not registered but information is received from external login
            externalLoginUser.Id = -1;
            return StatusCode(206, externalLoginUser);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status403Forbidden)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> Login(LoginInfo loginInfo)
      {

         try
         {
            IEnumerable<string> policies = AppFunc.GetCurrentRequestPolicies(Request, out AppTypes apptype);
            if (apptype == AppTypes.Invalid)
            {
               CoreFunc.Error(ref ErrorsList, "Unauthorised Application Access. 🤔");
               return Unauthorized(ErrorsList);
            }
            /// If email parameter is empty
            /// return "unauthorized" response (stop code execution)
            if (string.IsNullOrWhiteSpace(loginInfo.Email))
            {
               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, "Email is required!");
               return Unauthorized(ErrorsList);
            }

            /// Find the user with the provided email address
            User user = await _DbContext.Users
               .Include(u => u.Role)
               .Include(u => u.RegistrationMethod)
               .FirstOrDefaultAsync(r => r.Email.Equals(loginInfo.Email))
               .ConfigureAwait(false);
            /// if no user is found on the database
            if (user == null)
            {
               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, "Email not registered");
               return Unauthorized(ErrorsList);
            }
            if (!await IsUserPolicyAccepted(user, policies).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "You do not have permission to login here.");
               return Unauthorized(ErrorsList);
            }

            if (user.RegistrationMethod.Type != RegistrationTypes.Application)
            {
               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, $"Please use {user.RegistrationMethod.Type} to login.");
               return StatusCode(403, ErrorsList);
            }

            if (string.IsNullOrWhiteSpace(loginInfo.Password))
            {
               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, "Password is required!");
               return Unauthorized(ErrorsList);
            }

            /// Check if user's account is locked
            if (user.LockoutEnabled)
            {
               /// get the current lockout end dateTime
               var currentLockoutDate =
                   await _UserManager.GetLockoutEndDateAsync(user).ConfigureAwait(false);

               /// if the user's lockout is not expired (stop code execution)
               if (user.LockoutEnd > DateTimeOffset.UtcNow)
               {
                  /// in the case any exceptions return the following error
                  CoreFunc.Error(ref ErrorsList, string.Format("Account Locked for {0}"
                      , CoreFunc.CompareWithCurrentTime(user.LockoutEnd)));
                  return Unauthorized(ErrorsList);
               }
               /// else lockout time has expired
               // disable user lockout
               await _UserManager.SetLockoutEnabledAsync(user, false).ConfigureAwait(false);
               await _UserManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);
            }

            /// else user account is not locked
            // Attempt to sign in the user
            var SignInResult = await _SignInManager
                .PasswordSignInAsync(user, loginInfo.Password, loginInfo.RememberMe, false).ConfigureAwait(false);

            /// If password sign-in succeeds
            // responded ok 200 status code with
            //the user's role attached (stop code execution)
            if (!SignInResult.Succeeded)
            {
               /// else login attempt failed
               /// increase and update the user's failed login attempt by 1
               await _UserManager.AccessFailedAsync(user).ConfigureAwait(false);

               /// if failed login attempt is less than/ equal to 5 (stop code execution)
               if (user.AccessFailedCount <= 5)
               {
                  /// in the case any exceptions return the following error
                  CoreFunc.Error(ref ErrorsList, "Incorrect Password!");
                  return Unauthorized(ErrorsList);
               }

               /// else user has tried their password more than 15 times
               // lock the user and ask them to reset their password
               user.LockoutEnabled = true;
               user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(user.AccessFailedCount);

               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, string.Format("Account Locked for {0}"
                       , CoreFunc.CompareWithCurrentTime(user.LockoutEnd)));
               return Unauthorized(ErrorsList);
            }
            user.Role = (await _DbContext.Users.AsNoTracking()
              .Include(u => u.Role)
              .FirstOrDefaultAsync(u => u.Id == user.Id)
              .ConfigureAwait(false))
              ?.Role;

            return Ok(user);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]
      [HttpPost("Post/[action]")]
      public async Task<IActionResult> ConfirmCurrentUserPassword([FromBody] string password)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(password))
            {
               /// in the case any exceptions return the following error
               CoreFunc.Error(ref ErrorsList, "Password is required!");
               return Unauthorized(ErrorsList);
            }

            _ = int.TryParse(User.Claims
                .FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId);

            User user = await _DbContext.Users.Include(u => u.Role)
              .Include(u => u.RegistrationMethod)
              .FirstOrDefaultAsync(u => u.Id == userId)
              .ConfigureAwait(false);

            var result = await _SignInManager.CheckPasswordSignInAsync(user, password, false).ConfigureAwait(false);

            if (result.Succeeded)
            {
               return Ok(user);
            }
            else
            {
               CoreFunc.Error(ref ErrorsList, "Wrong Password");
               return Unauthorized(ErrorsList);
            }
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      private static async Task<User> GetGoogleUserInfo(ExternalLoginDetails externalLoginInfo)
      {
         ExternalEmailSecret googleSecrets = AppConst.Settings.ExternalLoginSecrets.FindObj(e => e.Provider.EqualCurrentCultureIgnoreCase("Google"));
         var caller = new HttpClient();
         var content = new StringContent($"client_id={googleSecrets?.ClientId}&client_secret={googleSecrets?.ClientSecret}&code={externalLoginInfo.Code}&grant_type=authorization_code&redirect_uri={externalLoginInfo.RedirectUrl.ToLower()}");
         content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
         // get the token from github API by using the provided "code", "state", "clientId" and "clientSecret".
         var tokenResult = await caller.PostAsync("https://oauth2.googleapis.com/token", content).ConfigureAwait(false);
         var tokenResultArray = JsonConvert.DeserializeObject<dynamic>(await tokenResult.Content.ReadAsStringAsync().ConfigureAwait(false));
         string tokenType = tokenResultArray.token_type;
         string accessToken = tokenResultArray.access_token;
         // Add the authorization information from the response above
         caller.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
         caller.DefaultRequestHeaders.Add("User-Agent", "OSnack");
         // get the user info by calling the github API with the above authorization
         var userInfoResult = await caller.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo").ConfigureAwait(false);
         var userInfoResultString = await userInfoResult.Content.ReadAsStringAsync().ConfigureAwait(false);
         var userInfoObj = JsonConvert.DeserializeObject<dynamic>(userInfoResultString);
         return new User
         {
            Email = (string)userInfoObj.email,
            FirstName = (string)userInfoObj.given_name,
            Surname = (string)userInfoObj.family_name,
            RegistrationMethod = new RegistrationMethod
            {
               ExternalLinkedId = (string)userInfoObj.id,
               RegisteredDate = DateTime.UtcNow,
               Type = externalLoginInfo.Type,
            }
         };
      }

      private static async Task<User> GetFacebookUserInfo(ExternalLoginDetails externalLoginInfo)
      {
         ExternalEmailSecret facebookSecrets = AppConst.Settings.ExternalLoginSecrets.FindObj(e => e.Provider.EqualCurrentCultureIgnoreCase("Facebook"));

         var caller = new HttpClient();
         // get the token from github API by using the provided "code", "state", "clientId" and "clientSecret".
         var tokenResult = await caller.GetAsync($"https://graph.facebook.com/v7.0/oauth/access_token?client_id={facebookSecrets?.ClientId}&redirect_uri={externalLoginInfo.RedirectUrl}&client_secret={facebookSecrets?.ClientSecret}&code={externalLoginInfo.Code}").ConfigureAwait(false);
         var tokenResultArray = JsonConvert.DeserializeObject<dynamic>(await tokenResult.Content.ReadAsStringAsync().ConfigureAwait(false));
         string tokenType = tokenResultArray.token_type;
         string accessToken = tokenResultArray.access_token;
         // Add the authorization information from the response above
         caller.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
         caller.DefaultRequestHeaders.Add("User-Agent", "OSnack");

         // get the user info by calling the github API with the above authorization
         var userInfoResult = await caller.GetAsync($"https://graph.facebook.com/me?fields=id,email,first_name,last_name&access_token={accessToken}").ConfigureAwait(false);
         var userInfoResultString = await userInfoResult.Content.ReadAsStringAsync().ConfigureAwait(false);
         var userInfoObj = JsonConvert.DeserializeObject<dynamic>(userInfoResultString);
         return new User
         {
            FirstName = (string)userInfoObj.first_name,
            Surname = (string)userInfoObj.last_name,
            Email = (string)userInfoObj.email,
            RegistrationMethod = new RegistrationMethod
            {
               ExternalLinkedId = (string)userInfoObj.id,
               RegisteredDate = DateTime.UtcNow,
               Type = externalLoginInfo.Type,
            }
         };
      }
   }
}
