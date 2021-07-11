using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Repositories.Interfaces;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class UnitOfWork : IUnitOfWork
   {
      private readonly OSnackDbContext _context;

      public UnitOfWork(OSnackDbContext context)
      {
         _context = context;
         Addresses = new AddressRepository(_context);
         Categories = new CategoryRepository(_context);
         Comments = new CommentRepository(_context);
         Communications = new CommunicationRepository(_context);
         Coupons = new Repository<Coupon>(_context);
         Dashboard = new DashboardRepository(_context);
         DeliveryOptions = new Repository<DeliveryOption>(_context);
         EmailTemplates = new EmailTemplateRepository(_context);
         Newsletters = new Repository<Newsletter>(_context);
         Orders = new OrderRepository(_context);
         Payments = new PaymentRepository(_context);
         Products = new ProductRepository(_context);
         RegistrationMethods = new Repository<RegistrationMethod>(_context);
         Tokens = new TokenRepository(_context);
         Roles = new Repository<Role>(_context);
         Users = new UserRepository(_context);
      }

      public IAddressRepository Addresses { get; private set; }
      public ICategoryRepository Categories { get; private set; }
      public ICommentRepository Comments { get; private set; }
      public ICommunicationRepository Communications { get; private set; }
      public IRepository<Coupon> Coupons { get; private set; }
      public IDashboardRepository Dashboard { get; private set; }
      public IRepository<DeliveryOption> DeliveryOptions { get; private set; }
      public IEmailTemplateRepository EmailTemplates { get; private set; }
      public IRepository<Newsletter> Newsletters { get; private set; }
      public IOrderRepository Orders { get; private set; }
      public IPaymentRepository Payments { get; private set; }
      public IProductRepository Products { get; private set; }
      public IRepository<RegistrationMethod> RegistrationMethods { get; private set; }
      public IRepository<Role> Roles { get; private set; }
      public ITokenRepository Tokens { get; private set; }
      public IUserRepository Users { get; private set; }


      public Task<int> CompleteAsync()
      {
         return _context.SaveChangesAsync();
      }

      public void Dispose()
      {
         _context.Dispose();
      }
   }
}
