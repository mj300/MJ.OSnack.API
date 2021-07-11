using OSnack.API.Database.Models;
using OSnack.API.Extras.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IOrderRepository : IRepository<Order>
   {

      Task AddOrder(Order orderData);
      Task<Order> GetOrder(string orderId);

      Task<Order> GetOrderWithPaymentAndUser(string orderId);

      Task<bool> HasDispute(bool isOpen);
      Task<bool> HasDispute(int userId, bool isOpen);

      Task<int> OrderCountAsync(Expression<Func<Order, bool>> predicate);
      Task<List<OrderStatusType>> AvailebeStatusTypes();
      Task<List<OrderStatusType>> AvailebeStatusTypes(int userId);

      Task<List<Order>> GetOrderByPageAsync(Expression<Func<Order, bool>> predicate, bool isSortAsce,
        string sortName, int selectedPage,
        int maxNumberPerItemsPage);

      Task<List<Order>> GetAllOrderByUserId(int userId);


   }

}
