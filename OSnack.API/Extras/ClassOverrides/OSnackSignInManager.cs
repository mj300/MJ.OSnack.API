using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OSnack.API.Database.Context.ClassOverrides
{
   public class OSnackSignInManager<TUser> : SignInManager<TUser> where TUser : class
   {
      public OSnackSignInManager(UserManager<TUser> userManager,
          IHttpContextAccessor contextAccessor,
          IUserClaimsPrincipalFactory<TUser> claimsFactory,
          IOptions<IdentityOptions> optionsAccessor,
          ILogger<SignInManager<TUser>> logger,
          IAuthenticationSchemeProvider schemes,
          IUserConfirmation<TUser> confirmation)
         : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
      { }
      /// <summary>
      /// Sign-out override method
      /// </summary>
      /// <returns></returns>
      public override async Task SignOutAsync()
      {
         /// Only sign-out from the applicationScheme
         await Context.SignOutAsync(IdentityConstants.ApplicationScheme).ConfigureAwait(false);
      }
   }
}
