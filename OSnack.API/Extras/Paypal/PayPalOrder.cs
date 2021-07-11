using PayPalCheckoutSdk.Orders;

using PayPalHttp;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Extras.Paypal
{
   public class PayPalOrder
   {
      //2. Set up your server to receive a call from the client
      /*
        Method to create order

        @param debug true = print response data
        @return HttpResponse<Order> response received from API
        @throws IOException Exceptions from API if any
      */
      public async static Task<HttpResponse> CreateOrder()
      {
         var request = new OrdersCreateRequest();
         request.Prefer("return=representation");
         request.RequestBody(BuildRequestBody());
         //3. Call PayPal to set up a transaction
         var response = await PayPalClient.client().Execute(request);

         return response;
      }

      /*
        Method to generate sample create order body with CAPTURE intent

        @return OrderRequest with created order request
       */
      private static OrderRequest BuildRequestBody()
      {
         OrderRequest orderRequest = new OrderRequest()
         {
            CheckoutPaymentIntent = "CAPTURE",

            ApplicationContext = new ApplicationContext
            {
               BrandName = "EXAMPLE INC",
               LandingPage = "BILLING",
               UserAction = "CONTINUE",
               ShippingPreference = "SET_PROVIDED_ADDRESS"
            },
            PurchaseUnits = new List<PurchaseUnitRequest>
        {
          new PurchaseUnitRequest{
            ReferenceId =  "PUHF",
            Description = "Sporting Goods",
            CustomId = "CUST-HighFashions",
            SoftDescriptor = "HighFashions",
            AmountWithBreakdown = new AmountWithBreakdown
            {
              CurrencyCode = "USD",
              Value = "230.00",
              AmountBreakdown = new AmountBreakdown
              {
                ItemTotal = new Money
                {
                  CurrencyCode = "USD",
                  Value = "180.00"
                },
                Shipping = new Money
                {
                  CurrencyCode = "USD",
                  Value = "30.00"
                },
                Handling = new Money
                {
                  CurrencyCode = "USD",
                  Value = "10.00"
                },
                TaxTotal = new Money
                {
                  CurrencyCode = "USD",
                  Value = "20.00"
                },
                ShippingDiscount = new Money
                {
                  CurrencyCode = "USD",
                  Value = "10.00"
                }
              }
            },
            Items = new List<Item>
            {
              new Item
              {
                Name = "T-shirt",
                Description = "Green XL",
                Sku = "sku01",
                UnitAmount = new Money
                {
                  CurrencyCode = "USD",
                  Value = "90.00"
                },
                Tax = new Money
                {
                  CurrencyCode = "USD",
                  Value = "10.00"
                },
                Quantity = "1",
                Category = "PHYSICAL_GOODS"
              },
              new Item
              {
                Name = "Shoes",
                Description = "Running, Size 10.5",
                Sku = "sku02",
                UnitAmount = new Money
                {
                  CurrencyCode = "USD",
                  Value = "45.00"
                },
                Tax = new Money
                {
                  CurrencyCode = "USD",
                  Value = "5.00"
                },
                Quantity = "2",
                Category = "PHYSICAL_GOODS"
              }
            },
            ShippingDetail = new ShippingDetail
            {
              Name = new Name
              {
                FullName = "John Doe"
              },
              AddressPortable = new AddressPortable
              {
                AddressLine1 = "123 Townsend St",
                AddressLine2 = "Floor 6",
                AdminArea2 = "San Francisco",
                AdminArea1 = "CA",
                PostalCode = "94107",
                CountryCode = "US"
              }
            }
          }
        }
         };

         return orderRequest;
      }
   }
}
