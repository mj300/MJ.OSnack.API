using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;
using OSnack.API.Repositories;
using OSnack.API.Repositories.Interfaces;
using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class CategoryController : ControllerBase
   {
      private IUnitOfWork _unitOfWork { get; }
      private LoggingService _LoggingService { get; }
      private IWebHostEnvironment _WebHost { get; }
      private List<Error> ErrorsList = new List<Error>();

      public CategoryController(OSnackDbContext db, IWebHostEnvironment webEnv)
      {
         _unitOfWork = new UnitOfWork(db);
         _WebHost = webEnv;
         _LoggingService = new LoggingService(db);
      }
   }
}
