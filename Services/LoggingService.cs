
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;

using System;
using System.Linq;
using System.Security.Claims;

namespace P8B.UK.API.Services
{
   public class LoggingService
   {
      private OSnackDbContext _DbContext { get; set; }

      public LoggingService() { }

      public LoggingService(OSnackDbContext dbContext) =>
         _DbContext = dbContext;

      internal AppLog Log(string message, AppLogType type, dynamic obj = null, ClaimsPrincipal userClaimsPrincipal = null)
      {
         AppLog log = new AppLog(message, type, obj, GetUser(userClaimsPrincipal));
         _DbContext.AppLogs.Add(log);
         if (log.User != null)
            _DbContext.Entry(log.User).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
         _DbContext.SaveChanges();
         return log;
      }
      internal string LogException(string message, dynamic obj = null, ClaimsPrincipal userClaimsPrincipal = null, AppLogType type = AppLogType.Exception)
      {
         AppLog log = new AppLog(message, type, obj, GetUser(userClaimsPrincipal));
         _DbContext.AppLogs.Add(log);
         _DbContext.SaveChanges();
         return CoreConst.CommonErrors.ServerError(log.Id);
      }

      private User GetUser(ClaimsPrincipal userClaimsPrincipal = null)
      {
         try
         {
            User user = null;
            if (userClaimsPrincipal != null)
               user = _DbContext.Users.SingleOrDefault(u => u.Id == AppFunc.GetUserId(userClaimsPrincipal));
            return user;
         }
         catch (Exception) { return null; }
      }

      internal string LogEmailFailure(string message, dynamic obj = null, User user = null, AppLogType type = AppLogType.EmailFailure)
      {
         AppLog log = new AppLog(message, type, obj, user);
         _DbContext.AppLogs.Add(log);
         _DbContext.SaveChanges();
         return CoreConst.CommonErrors.ServerError(log.Id);
      }

      internal void AddDbContext(OSnackDbContext dbContext) =>
         _DbContext = dbContext;
   }
}
