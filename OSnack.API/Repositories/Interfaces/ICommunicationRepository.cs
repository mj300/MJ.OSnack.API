using OSnack.API.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface ICommunicationRepository : IRepository<Communication>
   {
      Task<List<Communication>> GetAllCommunication(Expression<Func<Communication, bool>> predicate, bool isSortAsce,
          string sortName, int selectedPage,
          int maxNumberPerItemsPage);

      Task<Communication> GetCommunicationOrDisputeWithMessages(string communicationId);
      Task<Communication> GetCommunicationWithMessages(string communicationId);
      Task<Communication> GetDisputeWithMessages(int userId, string disputeId);
      Task<Communication> GetDisputeWithMessages(string disputeId);
      Task<List<Communication>> GetAllCommunicationWithEmail(string email);

      Task AddCommunication(Communication newCommunication);

      Task ChangeCommunicationEmail(string oldEmail, string newEmail);

   }
}
