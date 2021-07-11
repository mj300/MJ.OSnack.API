using OSnack.API.Database.Models;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface ITokenRepository : IRepository<Token>
   {
      Task<Token> GetToken(string pathName, string tokenValue);

   }
}
