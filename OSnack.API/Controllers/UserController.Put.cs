using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class UserController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("Put/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> UpdateUser([FromBody] User modifiedUser)
      {
         try
         {
            User user = await _unitOfWork.Users
               .GetUser(modifiedUser.Id)
               .ConfigureAwait(false);


            if (user == null)
            {
               CoreFunc.Error(ref ErrorsList, "User Not Found.");
               return StatusCode(412, ErrorsList);
            }

            modifiedUser.Role = await _unitOfWork.Roles
               .SingleOrDefaultAsync(r => r.Id == modifiedUser.Role.Id)
               .ConfigureAwait(false);

            ModelState.Clear();
            TryValidateModel(modifiedUser);

            ModelState.Remove("PasswordHash");
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            user.FirstName = modifiedUser.FirstName;
            user.Surname = modifiedUser.Surname;
            if (user.RegistrationMethod.Type == RegistrationTypes.Application && user.Email != modifiedUser.Email)
            {
               if (await _unitOfWork.Users.AnyAsync(d => d.NormalizedEmail == modifiedUser.Email.ToUpper()).ConfigureAwait(false))
               {
                  CoreFunc.Error(ref ErrorsList, "This email is already registered.");
                  return StatusCode(412, ErrorsList);
               }

               if (user.Email.ToLower() != modifiedUser.Email.ToLower())
               {

                  await _unitOfWork.Communications.ChangeCommunicationEmail(user.Email, modifiedUser.Email)
                     .ConfigureAwait(false);
                  var newsleter = await _unitOfWork.Newsletters.SingleOrDefaultAsync(n => n.Email == user.Email).ConfigureAwait(false);
                  if (newsleter != null)
                  {
                     newsleter.Email = modifiedUser.Email;
                     _unitOfWork.Newsletters.Modify(newsleter);
                  }

               }

               user.Email = modifiedUser.Email;
               user.NormalizedEmail = modifiedUser.Email.ToUpper();
            }
            user.PhoneNumber = modifiedUser.PhoneNumber;

            string oldAccessClaim = "";
            if (!user.Role.Equals(modifiedUser.Role))
            {
               oldAccessClaim = user.Role.AccessClaim;
               user.Role = modifiedUser.Role;
            }

            _unitOfWork.Users.Modify(user);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            if (!string.IsNullOrEmpty(oldAccessClaim))
            {
               IdentityResult removedClaimResult = await _UserManager.RemoveClaimAsync(user,
                  new Claim(AppConst.AccessClaims.Type, oldAccessClaim)
                  ).ConfigureAwait(false);

               if (removedClaimResult.Succeeded)
               {

                  IdentityResult addedClaimResult = await _UserManager.AddClaimAsync(user,
                     new Claim(AppConst.AccessClaims.Type, user.Role.AccessClaim)
                     ).ConfigureAwait(false);
               }
            }

            return Ok(user);
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
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("Put/UpdateCurrent")]
      [Authorize(AppConst.AccessPolicies.Official)]
      public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateCurrentUserData currentUserData)
      {
         try
         {
            _ = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId").Value, out int userId);
            User user = await _unitOfWork.Users
               .GetUser(userId)
               .ConfigureAwait(false);

            if (user == null || currentUserData.User.Id != user.Id)
            {
               CoreFunc.Error(ref ErrorsList, "Information access is denied.");
               return UnprocessableEntity(ErrorsList);
            }

            ModelState.Clear();
            TryValidateModel(currentUserData.User);
            ModelState.Remove("PasswordHash");
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            if (user.RegistrationMethod.Type == RegistrationTypes.Application && !await _UserManager.CheckPasswordAsync(user, currentUserData.CurrentPassword).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Current Password is incorrect.");
               return StatusCode(412, ErrorsList);
            }

            user.FirstName = currentUserData.User.FirstName;
            user.Surname = currentUserData.User.Surname;
            if (user.RegistrationMethod.Type == RegistrationTypes.Application && user.Email != currentUserData.User.Email)
            {
               if (await _unitOfWork.Users.AnyAsync(d => d.NormalizedEmail == currentUserData.User.Email.ToUpper()).ConfigureAwait(false))
               {
                  CoreFunc.Error(ref ErrorsList, "This email is already registered.");
                  return StatusCode(412, ErrorsList);
               }

               if (user.Email.ToLower() != currentUserData.User.Email.ToLower())
               {

                  await _unitOfWork.Communications.ChangeCommunicationEmail(user.Email, currentUserData.User.Email)
                     .ConfigureAwait(false);
                  var newsleter = await _unitOfWork.Newsletters.SingleOrDefaultAsync(n => n.Email == user.Email).ConfigureAwait(false);
                  if (newsleter != null)
                  {
                     newsleter.Email = currentUserData.User.Email;
                     _unitOfWork.Newsletters.Modify(newsleter);
                  }

               }

               user.Email = currentUserData.User.Email;
               user.NormalizedEmail = currentUserData.User.Email.ToUpper();
            }
            user.PhoneNumber = currentUserData.User.PhoneNumber;
            _unitOfWork.Users.Modify(user);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return Ok(user);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region ***  ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Secret)]
      [HttpPut("Put/[action]/{userId}/{lockoutEnabled}")]
      public async Task<IActionResult> UserLockout(int userId, bool lockoutEnabled)
      {
         try
         {
            User user = await _unitOfWork.Users.FindAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
               CoreFunc.Error(ref ErrorsList, "User not found");
               return StatusCode(412, ErrorsList);
            }
            user.LockoutEnabled = lockoutEnabled;
            _unitOfWork.Users.Modify(user);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            return Ok(user);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


      #region ***  ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("Put/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> ConfirmEmail([FromBody] string pathName)
      {
         try
         {
            string tokenValue = "";

            for (int i = pathName.Length - 1; i >= 0; i--)
            {
               if (pathName[i] == '/')
                  i = -1;
               else
                  tokenValue = pathName[i] + tokenValue;
            }

            Token token = await _unitOfWork.Tokens
               .GetToken(pathName, tokenValue)
               .ConfigureAwait(false);
            if (token == null || token.Type != Extras.CustomTypes.TokenTypes.ConfirmEmail)
            {
               ErrorsList.Add(new Error("0", "Invalid Request/Token."));
               return StatusCode(412, ErrorsList);
            }

            if (token.ExpiaryDateTime < DateTime.UtcNow)
            {
               ErrorsList.Add(new Error("0", "Token Expired"));
               return StatusCode(412, ErrorsList);
            }

            IdentityResult result = await _UserManager.ConfirmEmailAsync(token.User,
               await _UserManager.GenerateEmailConfirmationTokenAsync(token.User).ConfigureAwait(false))
               .ConfigureAwait(false);

            _unitOfWork.Tokens.Remove(token);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            if (result.Succeeded)
            {
               return Ok();
            }
            else
            {
               ErrorsList.Add(new Error("0", "Unable to process your request."));
               return StatusCode(412, ErrorsList);
            }
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
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]
      [HttpPut("Put/UpdateCurrentUser")]
      public async Task<IActionResult> UpdateCurrentUserPassword([FromBody] UpdateCurrentUserData data)
      {
         try
         {
            _ = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId").Value, out int userId);
            User user = await _unitOfWork.Users.FindAsync(userId);


            if (!await _UserManager.CheckPasswordAsync(user, data.CurrentPassword).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Current Password is incorrect.");
               return StatusCode(412, ErrorsList);
            }


            user.Password = data.User.Password;
            User result = await UpdatePassword(user).ConfigureAwait(false);
            if (result == null)
            {
               return StatusCode(412, ErrorsList);
            }
            return Ok(result);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("Put/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> UpdatePasswordWithToken([FromBody] dynamic data)
      {
         try
         {
            string passwordString = data.password;
            string pathName = data.pathName;
            bool justCheckToken = data.justCheckToken;

            string tokenValue = "";

            for (int i = pathName.Length - 1; i >= 0; i--)
            {
               if (pathName[i] == '/')
                  i = -1;
               else
                  tokenValue = pathName[i] + tokenValue;
            }

            Token token = await _unitOfWork.Tokens
               .GetToken(pathName, tokenValue)
               .ConfigureAwait(false);

            if (token == null || token.Type != Extras.CustomTypes.TokenTypes.ChangePassword)
            {
               ErrorsList.Add(new Error("0", "Invalid Request/Token."));
               return StatusCode(412, ErrorsList);
            }

            if (token.ExpiaryDateTime < DateTime.UtcNow)
            {
               ErrorsList.Add(new Error("0", "Token Expired"));
               return StatusCode(412, ErrorsList);
            }

            if (justCheckToken)
               return Ok("👌");


            token.User.Password = passwordString;

            User result = await UpdatePassword(token.User).ConfigureAwait(false);

            if (result == null)
               return StatusCode(412, ErrorsList);

            if (!result.EmailConfirmed)
            {
               await _UserManager.ConfirmEmailAsync(result,
                  await _UserManager.GenerateEmailConfirmationTokenAsync(result).ConfigureAwait(false))
                  .ConfigureAwait(false);
            }

            token.User = null;

            _unitOfWork.Tokens.Remove(token);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            await _SignInManager.SignInAsync(result, false).ConfigureAwait(false);

            return Ok("😜");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      private async Task<User> UpdatePassword(User selectedUser)
      {
         User userDetails = await _unitOfWork.Users.FindAsync(selectedUser.Id).ConfigureAwait(false);
         if (userDetails == null)
         {
            CoreFunc.Error(ref ErrorsList, "User not found!");
            return null;
         }
         string passResetToken = await _UserManager.GeneratePasswordResetTokenAsync(userDetails).ConfigureAwait(false);
         IdentityResult result = await _UserManager.ResetPasswordAsync(
                     userDetails, passResetToken, selectedUser.Password).ConfigureAwait(false);

         if (!result.Succeeded)
         {
            foreach (var error in result.Errors)
               ErrorsList.Add(new Error(error.Code, error.Description));
            return null;
         }

         return userDetails;
      }
   }
}
