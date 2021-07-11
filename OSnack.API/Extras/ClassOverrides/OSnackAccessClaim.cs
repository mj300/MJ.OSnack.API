using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSnack.API.Extras.ClassOverrides
{
   public class OSnackAccessClaim<TKey> : IdentityUserClaim<TKey> where TKey : IEquatable<TKey>
   {
      [NotMapped, JsonIgnore]
      public override int Id { get; set; }
      [ForeignKey("Id")]
      public override TKey UserId { get; set; }
      public override string ClaimType { get; set; } = "Role";
      public override string ClaimValue { get; set; }
   }
}
