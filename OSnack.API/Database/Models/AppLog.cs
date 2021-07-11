using Newtonsoft.Json;

using OSnack.API.Extras.CustomTypes;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OSnack.API.Database.Models
{
   [Table("AppLogs")]
   public class AppLog
   {
      [Key]
      public int Id { get; set; }

      [Column(TypeName = "nvarchar(30)")]
      [DataType(DataType.Date)]
      public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

      [Column(TypeName = "nvarchar(256)")]
      [Required(ErrorMessage = "Log Message Is Required. \n")]
      public string Message { get; set; }

      [Required(ErrorMessage = "Log Level Is Required. \n")]
      public AppLogType Type { get; set; }

      [Column(TypeName = "nvarchar(MAX)")]
      public string JsonObject { get; set; }

      [ForeignKey("UserId")]
      public User User { get; set; }

      public AppLog() { }
      public AppLog(string message, AppLogType type, dynamic obj = null, User user = null)
      {
         if (obj == null)
            obj = new { };
         Message = message;
         Type = type;
         if (obj != null)
            JsonObject = JsonConvert.SerializeObject(obj);
         if (user != null)
            User = user;
      }
   }
}
