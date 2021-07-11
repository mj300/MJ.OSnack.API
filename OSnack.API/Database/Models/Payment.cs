using Newtonsoft.Json;

using OSnack.API.Extras.Attributes;
using OSnack.API.Extras.CustomTypes;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{
   [Table("Payments")]
   public class Payment
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [Required(ErrorMessage = "Provider is required \n")]
      [Column(TypeName = "nvarchar(50)")]
      [StringLength(50, ErrorMessage = "Payment Provider Must be less than 50 Characters \n")]
      public string PaymentProvider { get; set; }

      [Required(ErrorMessage = "Reference is required \n")]
      [EmailTemplateVariable(Name = "PaymentRef")]
      [Column(TypeName = "nvarchar(50)")]
      [StringLength(50, ErrorMessage = "Reference Must be less than 50 Characters \n")]
      public string Reference { get; set; }

      [Required(ErrorMessage = "Payment Type is required \n")]
      public PaymentType Type { get; set; }

      [Column(TypeName = "nvarchar(256)")]
      [StringLength(256, ErrorMessage = "Email Must be less than 256 Characters \n")]
      public string Email { get; set; }

      [Required(ErrorMessage = "Date is required \n")]
      public DateTime DateTime { get; set; }

      [EmailTemplateVariable(Name = "Message")]
      [Column(TypeName = "nvarchar(500)")]
      [StringLength(500, ErrorMessage = "Message Must be less than 500 Characters \n")]
      public string Message { get; set; }


      [EmailTemplateVariable(Name = "PaymentDate")]
      [JsonIgnore, NotMapped]
      public string PaymentDate { get { return $"{DateTime.ToShortDateString()} {DateTime.ToShortTimeString()}"; } }

      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [EmailTemplateVariable(Name = "RefundAmount")]
      [Column(TypeName = "decimal(7,2)")]
      public decimal RefundAmount { get; set; }
      public DateTime? RefundDateTime { get; set; }




      [Required(ErrorMessage = "Order is required \n")]
      [ForeignKey("OrderId")]
      [JsonIgnore]
      public Order Order { get; set; }
   }
}
