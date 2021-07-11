using Newtonsoft.Json;
using OSnack.API.Extras;
using P8B.Core.CSharp.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Database.Models
{
   [Table("Comments")]
   public class Comment
   {
      [Key]
      [DefaultValue(0)]
      public int Id { get; set; }

      [Column(TypeName = "nvarchar(500)")]
      [StringLength(256, ErrorMessage = "Review Must be less than 500 Characters \n")]
      public string Description { get; set; }

      [Column(TypeName = "nvarchar(500)")]
      [StringLength(256, ErrorMessage = "Reply Must be less than 500 Characters \n")]
      public string Reply { get; set; }

      [ForeignKey("UserId")]
      [JsonIgnore]
      public User User { get; set; }

      [NotMapped]
      public string Name { get { return User != null ? $"{User.FirstName} {User.Surname.ToUpper().First()}" : ""; } }

      public DateTime Date { get; set; } = DateTime.UtcNow;

      [IntRange(ErrorMessage = "", MinValue = 0, MaxValue = 5)]
      public int Rate { get; set; }


      [ForeignKey("ProductId")]
      [JsonIgnore]
      public Product Product { get; set; }

      public async Task CencoredDescription()
      {
         if (!string.IsNullOrEmpty(Description))
         {
            string fileList = await File.ReadAllTextAsync(Path.Combine(@$"{Directory.GetCurrentDirectory()}\StaticFiles\list.txt")).ConfigureAwait(false);
            List<string> badWord = fileList.Split("\r\n").ToList();
            foreach (var word in Description.Split(" "))
            {
               if (badWord.Contains(word))
                  Description = Description.Replace(word, AppFunc.GetCencoredWord(word.Length));
            }
         }
      }
   }
}
