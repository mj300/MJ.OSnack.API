using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;
using OSnack.API.Extras;
using OSnack.API.Services;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class OrderController : ControllerBase
   {
      private OSnackDbContext _DbContext { get; }
      private LoggingService _LoggingService { get; }
      private EmailService _EmailService { get; }
      private List<Error> ErrorsList = new List<Error>();

      public OrderController(
          IWebHostEnvironment webEnv, OSnackDbContext db)
      {
         _DbContext = db;
         _LoggingService = new LoggingService(db);
         _EmailService = new EmailService(AppConst.Settings.EmailSettings, _LoggingService, webEnv, _DbContext);
      }


   }
}
