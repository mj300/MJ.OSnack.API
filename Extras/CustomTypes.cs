using OSnack.API.Database.Models;
using OSnack.API.Extras.Attributes;

namespace OSnack.API.Extras.CustomTypes
{
   public enum CouponType
   {
      FreeDelivery = 0,
      DiscountPrice = 1,
      PercentageOfTotal = 2
   }

   public enum EmailTemplateTypes
   {
      [EmailTemplateTypeRequiredClasses]
      DefaultTemplate = 0,

      //[EmailTemplateTypeRequiredClasses]
      //Others = 1,

      [EmailTemplateTypeRequiredClasses(typeof(User), typeof(Token))]
      EmailConfirmation = 2,

      [EmailTemplateTypeRequiredClasses(typeof(User), typeof(RegistrationMethod))]
      WelcomeExternalRegistration = 3,

      [EmailTemplateTypeRequiredClasses(typeof(User), typeof(Token), typeof(Role))]
      WelcomeNewEmployee = 4,

      [EmailTemplateTypeRequiredClasses(typeof(User), typeof(Token))]
      PasswordReset = 5,

      [EmailTemplateTypeRequiredClasses(typeof(Message), typeof(Communication))]
      MessageToAdmin = 6,

      [EmailTemplateTypeRequiredClasses(typeof(Message), typeof(Communication))]
      MessageToUser = 7,

      [EmailTemplateTypeRequiredClasses(typeof(Order), typeof(Payment), typeof(OrderItem))]
      OrderReceipt = 8,

      [EmailTemplateTypeRequiredClasses(typeof(Order), typeof(Payment), typeof(OrderItem))]
      OrderReceiptForAdmin = 11,

      [EmailTemplateTypeRequiredClasses(typeof(Order), typeof(Payment))]
      OrderCancellation = 9,

      [EmailTemplateTypeRequiredClasses(typeof(Order), typeof(Communication))]
      OrderDispute = 10,

      [EmailTemplateTypeRequiredClasses(typeof(Order), typeof(Communication))]
      OrderDisputeForAdmin = 12,
   }

   public enum TokenTypes
   {
      ChangePassword = 0,
      ConfirmEmail = 1,
      EndSubscription = 2
   }

   public enum OrderStatusType
   {
      InProgress = 0,
      Confirmed = 1,
      Canceled = 2,
      Delivered = 3,
      PartialyRefunded = 4,
      FullyRefunded = 5
   }

   public enum PaymentType
   {
      Complete = 0,
      Failed = 1,
      PartialyRefunded = 2,
      FullyRefunded = 3
   }

   public enum ProductUnitType
   {
      Kg = 0,
      Grams = 1,
      PerItem = 3
   }

   public enum AppLogType
   {
      Exception = 0,
      Information = 1,
      FileModification = 2,
      EmailFailure = 3,
      OrderException = 4,
      PaymentException = 5
   }

   public enum ContactType
   {
      Dispute = 0,
      Message = 1
   }

   public enum SalesPeriod
   {
      Daily = 0,
      Monthly = 1,
      Yearly = 2
   }

   public enum DisputeFilterTypes
   {
      None = 0,
      Open = 1,
      Close = 2,
      OpenAndClose = 3
   }

   public enum AppTypes
   {
      Client,
      Admin,
      Invalid
   }
}
