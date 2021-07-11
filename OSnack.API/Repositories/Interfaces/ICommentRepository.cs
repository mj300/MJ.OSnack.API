using OSnack.API.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface ICommentRepository : IRepository<Comment>
   {
      Task<Comment> GetSelectedComment(int userId, int productId);
      Task<List<Comment>> GetListOfCommentByProductId(int productId, int selectedPage, int maxItemsPerPage);
      Task<List<Comment>> GetListOfCommentByUserId(int userId);

      Task AddComment(Comment newComment);
   }
}
