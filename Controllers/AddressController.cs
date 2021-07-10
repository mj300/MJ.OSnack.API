using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;
using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class AddressController : ControllerBase
   {
      private OSnackDbContext _DbContext { get; }
      private LoggingService _LoggingService { get; }

      private List<Error> ErrorsList = new List<Error>();

      public AddressController(OSnackDbContext db)
      {
         _DbContext = db;
         _LoggingService = new LoggingService(db);
      }


      internal UnprocessableEntityObjectResult checkObjectIsNull()
      {
         CoreFunc.Error(ref ErrorsList, "Request cannot be process.");
         return UnprocessableEntity(ErrorsList);
      }


   }
}
