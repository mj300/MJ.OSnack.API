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
   public class OrderRepository : Repository<Order>, IOrderRepository
   {
      public OrderRepository(OSnackDbContext context)
          : base(context)
      {
      }

      public async Task AddOrder(Order orderData)
      {
         await _DbContext.Orders.AddAsync(orderData).ConfigureAwait(false);
         if (orderData.User != null)
         {
            foreach (Address address in orderData.User.Addresses)
            {
               _DbContext.Entry(address).State = EntityState.Unchanged;
            }
            _DbContext.Entry(orderData.User).State = EntityState.Unchanged;
         }
      }

      public async Task<List<OrderStatusType>> AvailebeStatusTypes()
      {
         return await _DbContext.Orders
                           .Select(o => o.Status)
                           .Distinct()
                           .ToListAsync()
                           .ConfigureAwait(false);
      }

      public async Task<List<OrderStatusType>> AvailebeStatusTypes(int userId)
      {
         return await _DbContext.Orders
                           .Include(o => o.User)
                           .Where(o => o.User.Id == userId)
                           .Select(o => o.Status)
                           .Distinct()
                           .ToListAsync()
                           .ConfigureAwait(false);
      }

      public async Task<List<Order>> GetAllOrderByUserId(int userId)
      {
         return await _DbContext.Orders
               .Include(o => o.User)
               .Include(o => o.OrderItems)
               .Include(o => o.Dispute)
               .ThenInclude(c => c.Messages)
               .Include(o => o.Payment)
               .Include(o => o.Coupon)
               .Where(u => u.User.Id == userId).ToListAsync().ConfigureAwait(false);
      }

      public async Task<Order> GetOrder(string orderId)
      {
         return await _DbContext.Orders.AsTracking()
                      .Include(o => o.Dispute).ThenInclude(c => c.Messages)
                      .Include(o => o.Payment).AsTracking()
                      .Include(o => o.OrderItems)
                      .SingleOrDefaultAsync(o => o.Id == orderId).ConfigureAwait(false);
      }

      public async Task<List<Order>> GetOrderByPageAsync(Expression<Func<Order, bool>> predicate, bool isSortAsce, string sortName, int selectedPage, int maxNumberPerItemsPage)
      {
         return await _DbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.Dispute)
                .ThenInclude(c => c.Messages)
                .Where(predicate)
                .OrderByDynamic(sortName, isSortAsce)
                .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                .Take(maxNumberPerItemsPage)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync()
                .ConfigureAwait(false);
      }


      public async Task<Order> GetOrderWithPaymentAndUser(string orderId)
      {
         return await _DbContext.Orders
               .Include(o => o.Payment)
               .Include(o => o.User)
               .SingleOrDefaultAsync(o => o.Id == orderId).ConfigureAwait(false);
      }

      public async Task<bool> HasDispute(bool isOpen)
      {
         return await _DbContext.Orders.Include(o => o.Dispute)
               .AnyAsync(o => o.Dispute.Status == isOpen).ConfigureAwait(false);
      }

      public async Task<bool> HasDispute(int userId, bool isOpen)
      {
         return await _DbContext.Orders
               .Include(o => o.Dispute)
               .Include(o => o.User)
               .Where(o => o.User.Id == userId)
               .AnyAsync(o => o.Dispute.Status == isOpen).ConfigureAwait(false);
      }

      public async Task<int> OrderCountAsync(Expression<Func<Order, bool>> predicate)
      {
         return await _DbContext.Orders
               .Include(o => o.User)
               .Include(o => o.Payment)
               .Include(o => o.Dispute)
               .CountAsync(predicate)
               .ConfigureAwait(false);
      }


   }
}
