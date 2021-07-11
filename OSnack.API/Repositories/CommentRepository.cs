using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Extras.CustomTypes;
using OSnack.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class CommentRepository : Repository<Comment>, ICommentRepository
   {
      public CommentRepository(OSnackDbContext context)
        : base(context)
      {
      }

      public async Task AddComment(Comment newComment)
      {
         await _DbContext.Comments.AddAsync(newComment).ConfigureAwait(false);
         _DbContext.Entry(newComment.User).State = EntityState.Unchanged;
         _DbContext.Entry(newComment.Product).State = EntityState.Unchanged;
      }

      public async Task<List<Comment>> GetListOfCommentByProductId(int productId, int selectedPage, int maxItemsPerPage)
      {
         return await _DbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Product)
            .Where(c => c.Product.Id == productId)
            .OrderBy(c => c.Date)
            .Skip((selectedPage - 1) * maxItemsPerPage)
            .Take(maxItemsPerPage)
            .ToListAsync()
            .ConfigureAwait(false);

      }

      public async Task<List<Comment>> GetListOfCommentByUserId(int userId)
      {
         return await _DbContext.Comments
                .Include(c => c.User)
                .Where(c => c.User.Id == userId).ToListAsync().ConfigureAwait(false);
      }

      public async Task<Comment> GetSelectedComment(int userId, int productId)
      {
         Comment selectComment = null;
         User user = await _DbContext.Users.Include(u => u.Orders)
                 .ThenInclude(o => o.OrderItems).SingleOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
         if (user.Orders.Any(o => o.Status == OrderStatusType.Delivered &&
                             o.OrderItems.Any(oi => oi.ProductId == productId)))
         {
            selectComment = await _DbContext.Comments.Include(c => c.User)
                            .SingleOrDefaultAsync(c => c.Product.Id == productId && c.User.Id == userId);
            if (selectComment == null)
               selectComment = new Comment()
               {
                  Id = 0,
                  Product = await _DbContext.Products.SingleOrDefaultAsync(p => p.Id == productId),
                  User = user
               };
         }

         return selectComment;
      }
   }
}
