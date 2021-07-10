using OSnack.API.Extras.Attributes;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.ModelsDependencies
{
   public abstract class OrderAddressBase
   {
      [EmailTemplateVariable(Name = "UserName")]
      [Column(TypeName = "nvarchar(200)")]
      [Required(ErrorMessage = "Name is Required \n")]
      [StringLength(200, ErrorMessage = "Must be less than 200 Characters \n")]
      public string Name { get; set; }

      [Required(ErrorMessage = "First Line is Required \n")]
      [Column(TypeName = "nvarchar(400)")]
      [EmailTemplateVariable(Name = "FirstLine")]
      [StringLength(400, ErrorMessage = "Must be less than 400 Characters \n")]
      public string FirstLine { get; set; }

      [Column(TypeName = "nvarchar(300)")]
      [EmailTemplateVariable(Name = "SecondLine")]
      [StringLength(400, ErrorMessage = "Must be less than 300 Characters \n")]
      public string SecondLine { get; set; } = "";

      [Column(TypeName = "nvarchar(50)")]
      [EmailTemplateVariable(Name = "City")]
      [Required(ErrorMessage = "City is Required \n")]
      [StringLength(50, ErrorMessage = "Must be less than 50 Characters \n")]
      public string City { get; set; }

      [Column(TypeName = "nvarchar(8)")]
      [EmailTemplateVariable(Name = "Postcode")]
      [Required(ErrorMessage = "Postcode is Required \n")]
      [StringLength(8, ErrorMessage = "Must be less than 8 Characters \n")]
      public string Postcode { get; set; }
   }
}
