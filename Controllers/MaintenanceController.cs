using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class MaintenanceController : ControllerBase
   {
      private OSnackDbContext _DbContext { get; }
      private LoggingService _LoggingService { get; }
      private List<Error> ErrorsList = new List<Error>();

      public MaintenanceController(OSnackDbContext db)
      {
         _DbContext = db;
         _LoggingService = new LoggingService(db);
      }
   }
}
