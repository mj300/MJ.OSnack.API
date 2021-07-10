using Newtonsoft.Json;

using OSnack.API.Extras.Attributes;
using OSnack.API.Extras.CustomTypes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OSnack.API.Database.Models
{
   [Table("Tokens")]
   public class Token
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [JsonIgnore]
      [Column(TypeName = "nvarchar(MAX)")]
      [Required(ErrorMessage = "Value is Required \n")]
      public string Value { get; set; }

      [Required(ErrorMessage = "Type is Required \n")]
      [JsonIgnore]
      public TokenTypes Type { get; set; }

      [EmailTemplateVariable(Name = "TokenUrl")]
      [Required(ErrorMessage = "Url is Required \n")]
      [JsonIgnore]
      public string Url { get; set; }

      [EmailTemplateVariable(Name = "ExpiaryDate")]
      [Column(TypeName = "nvarchar(30)")]
      [DataType(DataType.Date)]
      [Required(ErrorMessage = "Expiry Date is Required \n")]
      [JsonIgnore]
      public DateTime ExpiaryDateTime { get; set; }

      [JsonIgnore]
      [ForeignKey("UserId")]
      public User User { get; set; }

      [Column(TypeName = "nvarchar(256)")]
      [StringLength(256, ErrorMessage = "Email Must be less than 256 Characters \n")]
      public string Email { get; set; }

      [NotMapped]
      public string UrlDomain { get; set; }

      public Token() { }

      /// <summary>
      ///     Used to generate a URL to verify the user's request such as
      ///     password reset and user's email validation.
      /// </summary>
      /// <param name="user">Register the token with a specific user</param>
      /// <param name="ExpiaryDate">The expiry of the token</param>
      /// <param name="dbContext">DbContext to remove old tokens and save the new one</param>
      /// <param name="UrlPath">The path of the url e.g. "/reset/Password"</param>
      /// <param name="UrlDomain">The domain of the url e.g. "https://localhost:8080"</param>
      public void GenerateToken(User user, DateTime ExpiaryDate, OSnackDbContext dbContext, string UrlPath)
      {
         if (string.IsNullOrWhiteSpace(UrlDomain))
            throw new Exception("Domain URL Required");
         if (string.IsNullOrWhiteSpace(UrlPath))
            throw new Exception("Template Token URL Path Required");

         /// First check if the user has a token for the requested token type
         IEnumerable<Token> tokenValues = dbContext.Tokens
             .Where(vs => vs.User.Id == user.Id && vs.Type == Type)
             .AsEnumerable();

         /// If the user already has a token for the requested type
         /// then remove all of them
         if (tokenValues != null)
            dbContext.Tokens.RemoveRange(tokenValues);

         /// Create a new token
         User = user;
         Email = user.Email;
         ExpiaryDateTime = ExpiaryDate;
         Value = $"{Guid.NewGuid()}";
         Url = string.Format(@"{0}{1}/{2}", UrlDomain, UrlPath, Value);

         dbContext.Entry(User).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
         dbContext.Tokens.Add(this);
         dbContext.SaveChanges();
      }
   }
}
