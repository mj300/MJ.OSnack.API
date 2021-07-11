using Newtonsoft.Json;

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{
   [Table("Categories")]
   public class Category
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [Column(TypeName = "nvarchar(256)")]
      [Required(ErrorMessage = "Name is Required \n")]
      [StringLength(256, ErrorMessage = "Must be less than 256 Characters \n")]
      public string Name { get; set; }

      [Display(Name = "Display Image")]
      [StringLength(50, ErrorMessage = "Must be less than 50 Characters \n")]
      public string ImagePath { get; set; }

      [Display(Name = "Original Display Image")]
      [StringLength(50, ErrorMessage = "Must be less than 50 Characters \n")]
      public string OriginalImagePath { get; set; }

      [NotMapped]
      [Required(ErrorMessage = "Image is Required \n")]
      public string ImageBase64 { get; set; }

      [NotMapped]
      [Required(ErrorMessage = "Original Image is Required \n")]
      public string OriginalImageBase64 { get; set; }

      [JsonIgnore]
      [InverseProperty("Category")]
      public List<Product> Products { get; set; }

      [NotMapped]
      public int TotalProducts { get; set; } = 0;
   }
}
