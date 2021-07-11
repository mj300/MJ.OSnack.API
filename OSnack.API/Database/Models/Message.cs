using Newtonsoft.Json;
using OSnack.API.Extras.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{

   [Table("Messages")]
   public class Message
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [Column(TypeName = "nvarchar(100)")]
      [DataType(DataType.Date)]
      [EmailTemplateVariable(Name = "Date")]
      public DateTime Date { get; set; } = DateTime.UtcNow;


      [JsonIgnore, ForeignKey("CommunicationId")]
      public Communication Communication { get; set; }


      public bool IsCustomer { get; set; }


      [Column(TypeName = "nvarchar(500)")]
      [StringLength(500, ErrorMessage = "Body Must be less than 500 Characters \n")]
      [EmailTemplateVariable(Name = "MessageBody")]
      public string Body { get; set; }
   }
}
