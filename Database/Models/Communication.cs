using Newtonsoft.Json;

using OSnack.API.Extras.Attributes;
using OSnack.API.Extras.CustomTypes;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{


   [Table("Communications")]
   public class Communication
   {
      [Key]
      [Column(TypeName = "nvarchar(36)")]
      [StringLength(36, ErrorMessage = "Must be less than 36 Characters \n")]
      public string Id { get; set; } = Guid.NewGuid().ToString();

      [Required(ErrorMessage = "Contact Type is Required \n")]
      public ContactType Type { get; set; }

      public bool Status { get; set; }

      [Column(TypeName = "nvarchar(200)")]
      [StringLength(200, ErrorMessage = "Must be less than 200 Characters \n")]
      [EmailTemplateVariable(Name = "FullName")]
      public string FullName { get; set; }

      [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email \n")]
      [Required(ErrorMessage = "Email is Required \n")]
      [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
       ErrorMessage = "Invalid Email \n")]
      [Column(TypeName = "nvarchar(256)")]
      [StringLength(256, ErrorMessage = "Email Must be less than 256 Characters \n")]
      public string Email { get; set; }


      [ForeignKey("OrderId")]
      [JsonIgnore]
      public Order Order { get; set; }

      [NotMapped]
      public string Order_Id { get; set; }

      [InverseProperty("Communication")]
      public List<Message> Messages { get; set; }

      public DateTime Date { get; set; } = DateTime.UtcNow;


      [EmailTemplateVariable(Name = "CommunicationUrl")]
      [NotMapped, JsonIgnore]
      public string URL { get; private set; }

      [EmailTemplateVariable(Name = "Type")]
      [NotMapped, JsonIgnore]
      public string CommunicationType
      {
         get
         {
            switch (this.Type)
            {
               case ContactType.Dispute:
                  return Enum.Parse<ContactType>("Dispute").ToString("g");
               case ContactType.Message:
                  return Enum.Parse<ContactType>("Message").ToString("g");
               default:
                  return "";

            }
         }
      }

      public void SetURL(string url) => URL = $"{url}/{Id}";

      [NotMapped]
      public string captchaToken { get; set; }
   }

}
