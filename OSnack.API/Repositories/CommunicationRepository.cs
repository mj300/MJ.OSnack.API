using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Extras.CustomTypes;
using OSnack.API.Repositories.Interfaces;
using P8B.Core.CSharp.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class CommunicationRepository : Repository<Communication>, ICommunicationRepository
   {
      public CommunicationRepository(OSnackDbContext context)
        : base(context)
      {
      }

      public async Task AddCommunication(Communication newCommunication)
      {
         await _DbContext.Communications.AddAsync(newCommunication).ConfigureAwait(false);
         _DbContext.Entry(newCommunication.Order).State = EntityState.Unchanged;
         _DbContext.Entry(newCommunication.Order.User).State = EntityState.Unchanged;
         _DbContext.Entry(newCommunication.Order.Payment).State = EntityState.Unchanged;
      }

      public async Task ChangeCommunicationEmail(string oldEmail, string newEmail)
      {
         await _DbContext.Communications.Where(c => c.Email == oldEmail)
                   .ForEachAsync(c => c.Email = newEmail).ConfigureAwait(false);
      }

      public async Task<List<Communication>> GetAllCommunication(Expression<Func<Communication, bool>> predicate, bool isSortAsce,
          string sortName, int selectedPage,
          int maxNumberPerItemsPage)
      {
         return await _DbContext.Communications.Include(c => c.Order).ThenInclude(o => o.User)
               .Include(c => c.Messages)
               .Where(predicate)
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .ToListAsync()
                .ConfigureAwait(false);
      }

      public async Task<List<Communication>> GetAllCommunicationWithEmail(string email)
      {
         return await _DbContext.Communications
               .Include(c => c.Messages)
               .Where(c => c.Email.ToUpper() == email).ToListAsync().ConfigureAwait(false);
      }

      public async Task<Communication> GetCommunicationOrDisputeWithMessages(string communicationId)
      {
         return await _DbContext.Communications
                .Include(c => c.Messages)
                .SingleOrDefaultAsync(c => c.Id == communicationId);
      }

      public async Task<Communication> GetCommunicationWithMessages(string communicationId)
      {
         return await _DbContext.Communications
               .Include(c => c.Messages)
               .SingleOrDefaultAsync(c => c.Type == ContactType.Message && c.Id == communicationId)
               .ConfigureAwait(false);
      }

      public async Task<Communication> GetDisputeWithMessages(int userId, string disputeId)
      {
         return await _DbContext.Communications
               .Include(c => c.Order)
               .ThenInclude(o => o.User)
               .Include(c => c.Order)
               .ThenInclude(o => o.OrderItems)
               .Include(c => c.Messages)
               .SingleOrDefaultAsync(c => c.Type == ContactType.Dispute && c.Id == disputeId && c.Order.User.Id == userId)
               .ConfigureAwait(false);
      }

      public async Task<Communication> GetDisputeWithMessages(string disputeId)
      {
         return await _DbContext.Communications
                .Include(c => c.Order)
                .ThenInclude(o => o.User)
                .Include(c => c.Order)
                .ThenInclude(o => o.OrderItems)
                .Include(c => c.Messages)
                .SingleOrDefaultAsync(c => c.Type == ContactType.Dispute && c.Id == disputeId)
                .ConfigureAwait(false);
      }
   }
}
