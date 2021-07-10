﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System.Collections.Generic;

namespace OSnack.API.Controllers
{
   [Route("[controller]")]
   [AutoValidateAntiforgeryToken]
   [ApiControllerAttribute]
   public partial class EmailController : ControllerBase
   {
      private OSnackDbContext _DbContext { get; }
      private LoggingService _LoggingService { get; }
      private List<Error> ErrorsList = new List<Error>();
      private IWebHostEnvironment WebHost { get; }

      public EmailController(OSnackDbContext db, IWebHostEnvironment webEnv)
      {
         _DbContext = db;
         WebHost = webEnv;
         _LoggingService = new LoggingService(db);
      }
   }
}
