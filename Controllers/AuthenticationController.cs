using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;
using OSnack.API.Database.Context.ClassOverrides;
using OSnack.API.Database.Models;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class AuthenticationController : ControllerBase
   {
      private OSnackDbContext _DbContext { get; }
      private OSnackSignInManager<User> _SignInManager { get; }
      private LoggingService _LoggingService { get; }
      private UserManager<User> _UserManager { get; }
      private List<Error> ErrorsList = new List<Error>();
      private readonly IAntiforgery _Antiforgery;
      private readonly IWebHostEnvironment _WebHostingEnv;
      private readonly IAuthorizationService _AuthService;

      public AuthenticationController(OSnackDbContext db
         , OSnackSignInManager<User> sm
         , UserManager<User> um
         , IAntiforgery af
         , IWebHostEnvironment env
         , IAuthorizationService authService
         )
      {
         _DbContext = db;
         _SignInManager = sm;
         _UserManager = um;
         _Antiforgery = af;
         _WebHostingEnv = env;
         _AuthService = authService;
         _LoggingService = new LoggingService(db);
      }

      private async Task<bool> IsUserPolicyAccepted(User user, IEnumerable<string> policies)
      {
         ClaimsPrincipal claimsPrincipal = await _SignInManager.CreateUserPrincipalAsync(user).ConfigureAwait(false);

         foreach (string policy in policies)
         {
            AuthorizationResult authResult = await _AuthService.AuthorizeAsync(claimsPrincipal, policy).ConfigureAwait(false);
            if (authResult.Succeeded)
               return true;
         }
         return false;
      }
   }
}
