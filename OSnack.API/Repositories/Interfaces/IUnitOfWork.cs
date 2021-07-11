using OSnack.API.Database.Models;
using System;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IUnitOfWork : IDisposable
   {
      IAddressRepository Addresses { get; }
      ICategoryRepository Categories { get; }
      ICommentRepository Comments { get; }
      ICommunicationRepository Communications { get; }
      IRepository<Coupon> Coupons { get; }
      IDashboardRepository Dashboard { get; }
      IRepository<DeliveryOption> DeliveryOptions { get; }
      IEmailTemplateRepository EmailTemplates { get; }
      IRepository<RegistrationMethod> RegistrationMethods { get; }
      IRepository<Newsletter> Newsletters { get; }
      IOrderRepository Orders { get; }
      IPaymentRepository Payments { get; }
      IProductRepository Products { get; }
      IRepository<Role> Roles { get; }
      ITokenRepository Tokens { get; }
      IUserRepository Users { get; }
      Task<int> CompleteAsync();
   }
}
