using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class OrderController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> Put([FromBody] Order modifiedOrder)
      {
         try
         {
            Order originalOrder = await _DbContext.Orders.AsTracking()
                     .Include(o => o.Dispute).ThenInclude(c => c.Messages)
                     .Include(o => o.Payment).AsTracking()
                     .Include(o => o.OrderItems)
                     .SingleOrDefaultAsync(o => o.Id == modifiedOrder.Id).ConfigureAwait(false);

            if (originalOrder == null)
            {
               CoreFunc.Error(ref ErrorsList, "Order not exist");
               return StatusCode(412, ErrorsList);
            }

            if (!originalOrder.ChangeStatus(modifiedOrder.Status))
            {
               _LoggingService.Log(Request.Path, AppLogType.OrderException,
                        new { message = $"Order Status Mismatch.", originalOrder = originalOrder, modifiedOrder }, User);
               CoreFunc.Error(ref ErrorsList, "Cannot procces you order.Try again or contact Administrator.");
               return StatusCode(412, ErrorsList);
            }

            switch (originalOrder.Status)
            {
               case OrderStatusType.Confirmed:
                  originalOrder.ShippingReference = modifiedOrder.ShippingReference;
                  break;
               case OrderStatusType.Canceled:
               case OrderStatusType.PartialyRefunded:
               case OrderStatusType.FullyRefunded:
                  if (originalOrder.Status == OrderStatusType.PartialyRefunded &&
                     modifiedOrder.Payment.RefundAmount > originalOrder.TotalPrice)
                  {
                     _LoggingService.Log(Request.Path, AppLogType.OrderException,
                                         new { message = $"Refund value is more than Total Price.", originalOrder, modifiedOrder }, User);
                     CoreFunc.Error(ref ErrorsList, "Cannot procces you order.Try again or contact Administrator.");
                     return StatusCode(412, ErrorsList);
                  }

                  originalOrder.UpdatePayment(modifiedOrder.Payment);

                  if (!await originalOrder.RefundOrder())
                  {
                     await _DbContext.SaveChangesAsync().ConfigureAwait(false);
                     CoreFunc.Error(ref ErrorsList, "Refund cannot be verified.");
                     return UnprocessableEntity(ErrorsList);
                  }

                  if (originalOrder.Status == OrderStatusType.Canceled)
                     await RestoreProductQuantity(originalOrder);
                  break;
               case OrderStatusType.Delivered:
               case OrderStatusType.InProgress:
               default:
                  break;
            }

            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            if (originalOrder.Status == OrderStatusType.Canceled || originalOrder.Status == OrderStatusType.FullyRefunded
                || originalOrder.Status == OrderStatusType.PartialyRefunded)
               await _EmailService.OrderCancelationAsync(originalOrder, originalOrder.Dispute).ConfigureAwait(false);
            return Ok(originalOrder);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      private async Task RestoreProductQuantity(Order order)
      {
         List<Product> productList = await _DbContext.Products.AsTracking()
           .Where(p => order.OrderItems.Select(t => t.ProductId).Contains(p.Id) == true)
           .ToListAsync();
         foreach (var product in productList)
         {
            product.StockQuantity += order.OrderItems.SingleOrDefault(o => o.ProductId == product.Id).Quantity;
         }
      }
   }
}
