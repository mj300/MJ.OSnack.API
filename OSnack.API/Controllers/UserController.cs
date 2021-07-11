using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Repositories;
using OSnack.API.Repositories.Interfaces;
using OSnack.API.Services;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;

namespace OSnack.API.Controllers
{
   [Route("User")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class UserController : ControllerBase
   {
      private IUnitOfWork _unitOfWork { get; }
      private LoggingService _LoggingService { get; }
      private UserManager<User> _UserManager { get; }
      private SignInManager<User> _SignInManager { get; }
      private EmailService _EmailService { get; }
      private List<Error> ErrorsList = new List<Error>();

      public UserController(OSnackDbContext db,
          UserManager<User> um,
          IWebHostEnvironment webEnv,
          SignInManager<User> sm)
      {
         _unitOfWork = new UnitOfWork(db);
         _UserManager = um;
         _SignInManager = sm;
         _LoggingService = new LoggingService(db);
         _EmailService = new EmailService(AppConst.Settings.EmailSettings, _LoggingService, webEnv, db);
      }
   }
   public struct UpdateCurrentUserData
   {
      public User User { get; set; }
      public string CurrentPassword { get; set; }
   }
}
