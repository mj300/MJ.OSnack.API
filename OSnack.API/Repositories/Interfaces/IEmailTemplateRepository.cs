using OSnack.API.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IEmailTemplateRepository : IRepository<EmailTemplate>
   {
      Task<List<EmailTemplate>> GetAllEmailTemplateOrderByTemplateType();
   }
}
