using Newtonsoft.Json;

using OSnack.API.Database.ModelsDependencies;
using OSnack.API.Extras;
using OSnack.API.Extras.Attributes;
using OSnack.API.Extras.CustomTypes;
using OSnack.API.Extras.Paypal;

using PayPalCheckoutSdk.Orders;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Database.Models
{
   [Table("Orders")]
   public class Order : OrderAddressBase
   {
      [Key]
      [Column(TypeName = "nvarchar(7)")]
      [StringLength(7, ErrorMessage = "Must be less than 7 Characters \n")]
      [EmailTemplateVariable(Name = "OrderNumber")]
      public string Id { get; set; }

      [Column(TypeName = "nvarchar(100)")]
      [DataType(DataType.Date)]
      public DateTime Date { get; set; } = DateTime.UtcNow;

      [Required(ErrorMessage = "Status is required \n")]
      public OrderStatusType Status { get; set; }

      // [Required(ErrorMessage = "Delivery Option is required \n")]
      public DeliveryOption DeliveryOption { get; set; }

      [InverseProperty("Order")]
      public Communication Dispute { get; set; }


      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Column(TypeName = "decimal(7,2)")]
      public decimal DeliveryPrice { get; set; }

      /// <summary>
      /// <b>Not Mapped</b> Only used to pass address id from client side to server
      /// </summary>
      [Required(ErrorMessage = "Address Id is required \n")]
      [NotMapped]
      public int AddressId { get; set; }

      [Column(TypeName = "nvarchar(100)")]
      [StringLength(100, ErrorMessage = "Must be less than 100 Characters \n")]
      public string ShippingReference { get; set; }

      [ForeignKey("UserId")]
      [JsonIgnore]
      public User User { get; set; }

      [Column(Order = 0)]
      public int? UserId { get; set; }

      [Required(ErrorMessage = "Payment is required \n")]
      [InverseProperty("Order")]
      public Payment Payment { get; set; }

      [ForeignKey("Code")]
      public Coupon Coupon { get; set; }

      [InverseProperty("Order")]
      [EmailTemplateVariable(ListNames = new string[] { "StartRow", "EndRow" })]
      public ICollection<OrderItem> OrderItems { get; set; }

      [EmailTemplateVariable(Name = "TotalPrice")]
      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Column(TypeName = "decimal(7,2)")]
      [Required]
      public decimal TotalPrice { get; set; }

      [EmailTemplateVariable(Name = "SubTotal")]
      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Column(TypeName = "decimal(7,2)")]
      [Required]
      public decimal TotalItemPrice { get; set; }

      [EmailTemplateVariable(Name = "TotalShipping")]
      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Column(TypeName = "decimal(7,2)")]
      [Required]
      public decimal ShippingPrice { get; set; }

      [EmailTemplateVariable(Name = "TotalDiscount")]
      [DataType(DataType.Currency, ErrorMessage = "Invalid Currency \n")]
      [Column(TypeName = "decimal(7,2)")]
      [Required]
      public decimal TotalDiscount { get; set; }

      [EmailTemplateVariable(Name = "OrderStatus")]
      [JsonIgnore, NotMapped]
      public string StatusString { get { return Status.ToString(); } }
      internal bool ChangeStatus(OrderStatusType orderStatusType)
      {
         bool canChange = (Status, orderStatusType) switch
         {
            (OrderStatusType.InProgress, OrderStatusType.Confirmed) => true,
            (OrderStatusType.InProgress, OrderStatusType.Canceled) => true,
            (OrderStatusType.Delivered, OrderStatusType.FullyRefunded) => true,
            (OrderStatusType.Delivered, OrderStatusType.PartialyRefunded) => true,
            (OrderStatusType.Confirmed, OrderStatusType.PartialyRefunded) => true,
            (OrderStatusType.Confirmed, OrderStatusType.FullyRefunded) => true,
            (OrderStatusType.Confirmed, OrderStatusType.Delivered) => true,
            (_, _) => false
         };

         if (canChange)
            Status = orderStatusType;
         return canChange;
      }
      internal void CalculateTotalPrice()
      {
         CalculateDiscount();
         ShippingPrice = (decimal)DeliveryOption.Price;
         TotalPrice = TotalItemPrice + ShippingPrice - TotalDiscount;
      }
      internal void CalculateDiscount()
      {
         if (Coupon == null) return;
         if (!string.IsNullOrEmpty(Coupon.Code))
         {
            Coupon.MaxUseQuantity = Coupon.MaxUseQuantity - 1;

            switch (Coupon.Type)
            {
               case CouponType.FreeDelivery:
                  TotalDiscount = (decimal)DeliveryOption.Price;
                  break;
               case CouponType.DiscountPrice:
                  TotalDiscount = (decimal)Coupon.DiscountAmount;
                  break;
               case CouponType.PercentageOfTotal:
                  TotalDiscount = Math.Round((((decimal)Coupon.DiscountAmount * TotalItemPrice) / 100), 2);
                  break;
               default:
                  break;
            }
         }
      }

      internal List<Item> ConvertItem()
      {
         List<Item> orderItems = new List<Item>();
         foreach (var orderItem in OrderItems)
         {
            orderItems.Add(new PayPalCheckoutSdk.Orders.Item()
            {
               Name = orderItem.Name,
               Quantity = orderItem.Quantity.ToString(),
               UnitAmount = new PayPalCheckoutSdk.Orders.Money()
               {
                  CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
                  Value = orderItem.Price?.ToString("0.00")
               }
            });
         }
         return orderItems;
      }

      internal void UpdatePayment(Payment payment)
      {
         if (payment.RefundAmount == TotalPrice)
            Status = OrderStatusType.FullyRefunded;

         Payment.RefundDateTime = DateTime.UtcNow;
         Payment.Message = payment.Message;
         Payment.RefundAmount = Status == OrderStatusType.PartialyRefunded ? payment.RefundAmount : TotalPrice;
         Payment.Type = Status == OrderStatusType.PartialyRefunded ? PaymentType.PartialyRefunded : PaymentType.FullyRefunded;
      }

      internal void UpdateAddress(Address address)
      {
         Name = address.Name;
         FirstLine = address.FirstLine;
         SecondLine = address.SecondLine;
         City = address.City;
         Postcode = address.Postcode;
         User = address.User;
      }

      internal async Task<PayPalCheckoutSdk.Orders.Order> ConvertToPayPalOrder()
      {

         OrderRequest orderRequest = new OrderRequest();

         orderRequest.CheckoutPaymentIntent = "CAPTURE";
         orderRequest.ApplicationContext = new ApplicationContext()
         {
            BrandName = AppConst.Settings.BrandName,
            UserAction = "CONTINUE",
            ShippingPreference = "SET_PROVIDED_ADDRESS",
         };

         orderRequest.PurchaseUnits = new List<PurchaseUnitRequest>()
         {
            new PurchaseUnitRequest (){
               ReferenceId =  "PUHF",
               Description = "Snacks and food products",
               CustomId = "CUST-HighFashions",
               SoftDescriptor = "Snacks",
               AmountWithBreakdown = new AmountWithBreakdown()
               {
                  CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
                  Value = TotalPrice.ToString("0.00"),
                  AmountBreakdown = new AmountBreakdown()
                  {
                       ItemTotal = new Money()
                       {
                           CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
                           Value = TotalItemPrice.ToString("0.00")
                       },
                       Shipping = new Money()
                       {
                           CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
                           Value = ShippingPrice.ToString("0.00")

                       },
                       Discount = new Money()
                       {
                           CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
                           Value = TotalDiscount.ToString("0.00")
                       }
                  }
               } ,
               Items = ConvertItem(),
            }
         };
         if (User != null)
         {
            orderRequest.PurchaseUnits.FirstOrDefault().ShippingDetail = new ShippingDetail()
            {
               Name = new Name()
               {
                  FullName = $"{User.FirstName} {User.Surname}"
               },
               AddressPortable = new AddressPortable()
               {
                  AddressLine1 = FirstLine,
                  AddressLine2 = SecondLine,
                  PostalCode = Postcode,
                  CountryCode = "GB",
                  AdminArea1 = "UK",
                  AdminArea2 = City
               },

            };
            orderRequest.Payer = new Payer()
            {
               Email = User.Email,
               Name = new Name()
               {
                  GivenName = User.FirstName,
                  Surname = User.Surname
               },
               AddressPortable = new AddressPortable()
               {
                  AddressLine1 = FirstLine,
                  AddressLine2 = SecondLine,
                  PostalCode = Postcode,
                  CountryCode = "GB",
                  AdminArea2 = City
               }
            };
         }
         else
         {
            orderRequest.ApplicationContext.ShippingPreference = "GET_FROM_FILE";
         }
         OrdersCreateRequest request = new OrdersCreateRequest();

         request.Prefer("return=representation");

         request.RequestBody(orderRequest);
         var response = await PayPalClient.client().Execute(request).ConfigureAwait(false);

         PayPalCheckoutSdk.Orders.Order paypalOrder = response.Result<PayPalCheckoutSdk.Orders.Order>();

         return paypalOrder;
      }

      internal async Task<bool> CapturePaypalPayment(string paypalId)
      {
         Payment = new Payment()
         {
            PaymentProvider = "PayPal",
            Reference = paypalId,
         };
         var request = new OrdersCaptureRequest(paypalId);
         request.Prefer("return=representation");
         request.RequestBody(new OrderActionRequest());
         var response = await PayPalClient.client().Execute(request);
         var paypalOrder = response.Result<PayPalCheckoutSdk.Orders.Order>();
         if (!paypalOrder.Status.Equals("COMPLETED"))
            return false;
         Status = OrderStatusType.InProgress;
         Payment.Reference = paypalOrder.PurchaseUnits.FirstOrDefault().Payments.Captures.FirstOrDefault().Id;
         Payment.DateTime = DateTime.Parse(paypalOrder.UpdateTime);
         Payment.Type = PaymentType.Complete;
         Payment.Email = paypalOrder.Payer.Email;
         var purchaseUnit = paypalOrder.PurchaseUnits.FirstOrDefault();
         Name = purchaseUnit.ShippingDetail.Name.FullName;
         FirstLine = purchaseUnit.ShippingDetail.AddressPortable.AddressLine1;
         SecondLine = purchaseUnit.ShippingDetail.AddressPortable.AddressLine2;
         if (purchaseUnit.ShippingDetail.AddressPortable.AdminArea1 != null)
            City = purchaseUnit.ShippingDetail.AddressPortable.AdminArea1;
         if (purchaseUnit.ShippingDetail.AddressPortable.AdminArea2 != null)
            City = purchaseUnit.ShippingDetail.AddressPortable.AdminArea2;
         Postcode = purchaseUnit.ShippingDetail.AddressPortable.PostalCode;
         return true;
      }

      internal async Task<bool> RefundOrder()
      {
         PayPalCheckoutSdk.Payments.RefundRequest refundRequest = new PayPalCheckoutSdk.Payments.RefundRequest()
         {
            Amount = new PayPalCheckoutSdk.Payments.Money
            {
               CurrencyCode = AppConst.Settings.PayPal.CurrencyCode,
               Value = Payment.RefundAmount.ToString("0.00")
            },
            NoteToPayer = Payment.Message
         };
         var request = new PayPalCheckoutSdk.Payments.CapturesRefundRequest(Payment.Reference);
         request.Prefer("return=representation");
         request.RequestBody(refundRequest);
         var response = await PayPalClient.client().Execute(request);
         var refund = response.Result<PayPalCheckoutSdk.Payments.Refund>();
         if (!refund.Status.Equals("COMPLETED"))
            return false;
         if (Dispute != null)
         {
            Dispute.Messages.Add(new Message()
            {
               Body = $"{Dispute.CommunicationType} was closed",
               IsCustomer = false
            });
            Dispute.Status = false;
         }
         return true;

      }
   }
}
