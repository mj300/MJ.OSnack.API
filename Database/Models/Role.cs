using Newtonsoft.Json;

using OSnack.API.Extras.Attributes;

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Database.Models
{
   [Table("Roles")]
   public class Role
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [Column(TypeName = "nvarchar(30)")]
      [EmailTemplateVariable(Name = "RoleName")]
      [Required(ErrorMessage = "Role Name is Required \n")]
      public string Name { get; set; }

      [Column(TypeName = "nvarchar(30)")]
      [Required(ErrorMessage = "Access Claim is Required \n")]
      [ValidateAccessClaim]
      public string AccessClaim { get; set; }

      [JsonIgnore]
      [InverseProperty("Role")]
      public ICollection<User> Users { get; set; }
   }
}
