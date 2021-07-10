using Microsoft.EntityFrameworkCore.Metadata.Internal;

using Newtonsoft.Json;
using P8B.Core.CSharp.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{

   [Table("DeliveryOption")]
   public class DeliveryOption
   {

      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [StringLength(50, ErrorMessage = "Must be less than 50 Characters \n")]
      [Required(ErrorMessage = "Name Is Required \n")]
      public string Name { get; set; }


      [Column(TypeName = "decimal(7,2)")]
      [Required(ErrorMessage = "Price Is Required \n")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Price should not be negative. \n")]
      public decimal? Price { get; set; }


      [Column(TypeName = "decimal(7,2)")]
      [Required(ErrorMessage = "Minimum Order Total Is Required \n")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Minimum Order Total should not be negative. \n")]
      public decimal? MinimumOrderTotal { get; set; }

      public bool IsPremitive { get; set; } = false;

      [JsonIgnore]
      [InverseProperty("DeliveryOption")]
      public ICollection<Order> Orders { get; set; }
   }
}
