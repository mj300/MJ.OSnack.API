using Newtonsoft.Json;
using OSnack.API.Extras.Attributes;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.JsonConvertor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.ModelsDependencies
{
   public abstract class OrderProductBase
   {
      [EmailTemplateVariable(Name = "ProductName")]
      [Column(TypeName = "nvarchar(50)")]
      [Required(ErrorMessage = "Product Name is Required \n")]
      [StringLength(50, ErrorMessage = "Must be less than 50 Characters \n")]
      public string Name { get; set; }

      [EmailTemplateVariable(Name = "ProductPrice")]
      [Column(TypeName = "decimal(7,2)")]
      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Required(ErrorMessage = "Price is Required \n")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Price should not be negative. \n")]
      [Range(0, 99999, ErrorMessage = "Price Must be less than 99999")]
      public decimal? Price { get; set; }

      [EmailTemplateVariable(Name = "UnitQuantity")]
      [Required(ErrorMessage = "Unit Quantity is Required \n")]
      [JsonConverter(typeof(StrictIntConverter))]
      public int? UnitQuantity { get; set; }

      [EmailTemplateVariable(Name = "UnitType")]
      [Required(ErrorMessage = "Unit Type is Required \n")]
      public ProductUnitType? UnitType { get; set; }
   }
}
