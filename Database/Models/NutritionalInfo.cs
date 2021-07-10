using Newtonsoft.Json;

using P8B.Core.CSharp.Attributes;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{
   public class NutritionalInfo
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      public int PerGram { get; set; } = 100;

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "EnergyKJ Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "EnergyKJ Must be positive number \n", AllowNull = true)]
      public decimal? EnergyKJ { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "EnergyKcal Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? EnergyKcal { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "Fat Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? Fat { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "SaturateFat Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? SaturateFat { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "Carbohydrate Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? Carbohydrate { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "carbohydrateSugar Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? CarbohydrateSugar { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "Fibre Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? Fibre { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "Protein Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? Protein { get; set; }

      [Column(TypeName = "decimal(5,2)")]
      [Range(0, 999, ErrorMessage = "Salt Must be less than 999")]
      [PositiveDecimalIncludingZero(ErrorMessage = "Must be positive number \n", AllowNull = true)]
      public decimal? Salt { get; set; }

      [JsonIgnore]
      [ForeignKey("ProductId")]
      [Required]
      public Product Product { get; set; }
   }
}
