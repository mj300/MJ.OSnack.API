using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.JsonConvertor;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class UserController
   {
      private bool IsUserCreated { get; set; }

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> CreateUser([FromBody] User newUser)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(newUser.Password))
               newUser.Password = CoreFunc.StringGenerator(10, 3, 3, 2, 2);

            if (newUser.Role.Id == 0)
               newUser.Role = null;
            TryValidateModel(newUser);

            ModelState.Remove("Role.Name");
            ModelState.Remove("Role.AccessClaim");
            ModelState.Remove("PasswordHash");
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            newUser.Role = await _unitOfWork.Roles.FindAsync(newUser.Role.Id).ConfigureAwait(false);

            IActionResult result = await PrivateCreateUser(newUser).ConfigureAwait(false);

            if (IsUserCreated)
            {
               Request.Headers.TryGetValue("Origin", out StringValues OriginValue);
               await _EmailService
                  .WelcomeNewEmployeeAsync(newUser, OriginValue)
                  .ConfigureAwait(false);
            }
            newUser.Password = string.Empty;

            return result;
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));

            if (IsUserCreated)
            {
               _unitOfWork.Users.Remove(newUser);
               await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            return StatusCode(417, ErrorsList);
         }
      }


      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]/{subscribeNewsLetter}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> CreateCustomer([FromBody] User newCustomer, bool subscribeNewsLetter)
      {
         try
         {
            if (newCustomer.RegistrationMethod == null)
               newCustomer.RegistrationMethod = new RegistrationMethod
               {
                  Type = RegistrationTypes.Application
               };

            newCustomer.Role = await _unitOfWork.Roles
                .SingleOrDefaultAsync(r => r.AccessClaim.Equals(AppConst.AccessClaims.Customer))
                .ConfigureAwait(false);

            IActionResult result = await PrivateCreateUser(newCustomer).ConfigureAwait(false);

            if (!IsUserCreated)
               return result;

            switch (newCustomer.RegistrationMethod.Type)
            {
               case RegistrationTypes.Application:
                  Request.Headers.TryGetValue("Origin", out StringValues OriginValue);
                  await _EmailService.EmailConfirmationAsync(newCustomer, OriginValue)
                     .ConfigureAwait(false);
                  break;
               case RegistrationTypes.Facebook:
               case RegistrationTypes.Google:
                  newCustomer.RegistrationMethod.RegistrationType = Enum.GetName(newCustomer.RegistrationMethod.Type);
                  await _EmailService.WelcomeExternalRegistrationAsync(newCustomer)
                     .ConfigureAwait(false);
                  break;
            }

            if (subscribeNewsLetter)
            {
               //_DbContext.DetachAllEntities();
               await _unitOfWork.Newsletters.AddAsync(new Newsletter { Email = newCustomer.Email });
               await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            await _SignInManager.SignInAsync(newCustomer, false).ConfigureAwait(false);
            newCustomer.Password = string.Empty;

            return Created("", newCustomer);
         }

         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            if (IsUserCreated)
            {
               _unitOfWork.Users.Remove(newCustomer);
               await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [ProducesResponseType(StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(email))
            {
               CoreFunc.Error(ref ErrorsList, "Email is required!");
               return StatusCode(412, ErrorsList);
            }

            User user = await _UserManager
                    .FindByEmailAsync(email).ConfigureAwait(false);

            if (user == null)
            {
               CoreFunc.Error(ref ErrorsList, "Email not registered");
               return StatusCode(412, ErrorsList);
            }
            RegistrationMethod registrationMethod = await _unitOfWork.RegistrationMethods.SingleOrDefaultAsync(rm => rm.User.Id == user.Id).ConfigureAwait(false);
            if (registrationMethod.Type != RegistrationTypes.Application)
            {
               CoreFunc.Error(ref ErrorsList, $"Your account is linked to your {registrationMethod.Type} account.");
               return StatusCode(412, ErrorsList);
            }

            if (user.LockoutEnabled)
            {
               var currentLockoutDate =
                   await _UserManager.GetLockoutEndDateAsync(user).ConfigureAwait(false);

               if (user.LockoutEnd > DateTimeOffset.UtcNow)
               {
                  CoreFunc.Error(ref ErrorsList, string.Format("Account Locked for {0}"
                      , CoreFunc.CompareWithCurrentTime(user.LockoutEnd)));
                  return StatusCode(412, ErrorsList);
               }
               await _UserManager.SetLockoutEnabledAsync(user, false).ConfigureAwait(false);
               await _UserManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);
            }

            Request.Headers.TryGetValue("Origin", out StringValues OriginValue);
            if (!await _EmailService.PasswordResetAsync(user, OriginValue).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Unable to send email.");
               return StatusCode(417, ErrorsList);
            }

            return Created("", "Created");
         }

         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


      #region *** 200 OK, 417 ExpectationFailed ***
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPost("Post/[action]")]
      [Authorize(AppConst.AccessPolicies.Official)]
      public async Task<IActionResult> DownloadData([FromBody] string currentPassword)
      {
         try
         {
            User user = await _unitOfWork.Users
               .GetUser(AppFunc.GetUserId(User)).ConfigureAwait(false);



            if (user.RegistrationMethod.Type == RegistrationTypes.Application
               && !await _UserManager.CheckPasswordAsync(user, currentPassword).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Current Password is incorrect.");
               return StatusCode(412, ErrorsList);
            }

            List<Order> orders = await _unitOfWork.Orders
               .GetAllOrderByUserId(AppFunc.GetUserId(User)).ConfigureAwait(false);

            List<Communication> questions = await _unitOfWork.Communications
               .GetAllCommunicationWithEmail(user.NormalizedEmail).ConfigureAwait(false);

            List<Comment> comments = await _unitOfWork.Comments
               .GetListOfCommentByUserId(user.Id).ConfigureAwait(false);

            List<Address> addresses = await _unitOfWork.Addresses
               .GetAllAddressByUserId(user.Id).ConfigureAwait(false);

            dynamic userData = new { userInfo = user, orders, questions, comments, addresses };
            return Ok(JsonConvert.SerializeObject(userData, Formatting.Indented,
            new JsonSerializerSettings
            {
               Converters = new List<JsonConverter> { new StringEnumConverter(), new DecimalFormatConverter() },
               ContractResolver = new DynamicContractResolver("Id", "Password", "AccessClaim", "OrderLength", "HasOrder"
               , "DeliveryOption", "Order_Id", "captchaToken", "AddressId", "UserId", "ProductId", "ImagePath", "ExternalLinkedId")
            }));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      private async Task<IActionResult> PrivateCreateUser(User newUser)
      {
         try
         {
            newUser.PasswordHash = newUser.Password;
            if (newUser.RegistrationMethod.Type != RegistrationTypes.Application)
            {
               newUser.PasswordHash = CoreFunc.StringGenerator(40, 10, 10, 10, 10);
               newUser.EmailConfirmed = true;
            }
            ModelState.Clear();
            if (!TryValidateModel(newUser))
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }
            if (await _unitOfWork.Users.AnyAsync(u => u.NormalizedEmail == newUser.Email.ToUpper()).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "This email is already registered.");
               return StatusCode(412, ErrorsList);
            }

            newUser.Id = 0;
            IdentityResult newUserResult = await _UserManager.CreateAsync(newUser, newUser.PasswordHash)
                                                            .ConfigureAwait(false);
            if (!newUserResult.Succeeded)
            {
               foreach (var error in newUserResult.Errors)
               {
                  CoreFunc.Error(ref ErrorsList, error.Description, error.Code);
               }
               return StatusCode(417, ErrorsList);
            }
            IdentityResult addedClaimResult = await _UserManager.AddClaimAsync(
                    newUser,
                    new Claim(AppConst.AccessClaims.Type, newUser.Role.AccessClaim)
                ).ConfigureAwait(false);
            if (!addedClaimResult.Succeeded)
            {
               _unitOfWork.Users.Remove(newUser);
               await _unitOfWork.CompleteAsync().ConfigureAwait(false);
               CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, null, User));
               return StatusCode(417, ErrorsList);
            }
            await _UserManager.SetLockoutEnabledAsync(newUser, false);
            IsUserCreated = true;
            return Created("Success", newUser);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
