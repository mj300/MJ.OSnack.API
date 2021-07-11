using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;

namespace OSnack.API.Repositories
{
   public class PaymentRepository : Repository<Payment>, IPaymentRepository
   {
      public PaymentRepository(OSnackDbContext context)
          : base(context)
      {
      }


   }
}
