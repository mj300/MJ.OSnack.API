using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
   {
      public EmailTemplateRepository(OSnackDbContext context)
          : base(context)
      {
      }

      public async Task<List<EmailTemplate>> GetAllEmailTemplateOrderByTemplateType()
      {
         return await _DbContext.EmailTemplates
                .OrderByDescending(et => et.TemplateType)
                .ToListAsync()
                .ConfigureAwait(false);
      }
   }
}
