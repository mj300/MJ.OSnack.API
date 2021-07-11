
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{
   public class Newsletter
   {


      [Key]
      [Column(TypeName = "nvarchar(36)")]
      [StringLength(36, ErrorMessage = "Must be less than 36 Characters \n")]
      public string Id { get; set; } = Guid.NewGuid().ToString();


      [Column(TypeName = "nvarchar(256)")]
      [StringLength(256, ErrorMessage = "Email Must be less than 256 Characters \n")]
      [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email \n")]
      [Required(ErrorMessage = "Email is Required \n")]
      [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
          ErrorMessage = "Invalid Email \n")]
      public string Email { get; set; }


   }
}
