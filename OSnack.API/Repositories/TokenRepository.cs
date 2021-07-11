using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class TokenRepository : Repository<Token>, ITokenRepository
   {
      public TokenRepository(OSnackDbContext context)
          : base(context)
      {
      }

      public async Task<Token> GetToken(string pathName, string tokenValue)
      {
         return await _DbContext.Tokens
               .Include(t => t.User)
               .FirstOrDefaultAsync(t => t.Url.Contains(pathName) && t.Value.Equals(tokenValue))
               .ConfigureAwait(false);
      }
   }
}
