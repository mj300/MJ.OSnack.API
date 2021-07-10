using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class DashboardController
   {
      #region *** ***
      [MultiResultPropertyNames("newOrderCount", "openDisputeCount", "openMessageCount", "totalSales")]
      [ProducesResponseType(typeof(MultiResult<int, int, int, decimal>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> Summary()
      {
         try
         {
            int newOrderCount = await _DbContext.Orders
               .CountAsync(o => o.Status == OrderStatusType.InProgress).ConfigureAwait(false);
            int openDisputeCount = await _DbContext.Communications
               .CountAsync(o => o.Type == ContactType.Dispute && o.Status == true).ConfigureAwait(false);
            int openMessageCount = await _DbContext.Communications
               .CountAsync(o => o.Type == ContactType.Message && o.Status == true).ConfigureAwait(false);
            decimal totalPrice = await _DbContext.Orders
               .Where(o => o.Status == OrderStatusType.InProgress
                    || o.Status == OrderStatusType.Confirmed
                    || o.Status == OrderStatusType.Delivered
                    || o.Status == OrderStatusType.PartialyRefunded)
               .SumAsync(o => o.TotalPrice).ConfigureAwait(false);
            decimal totalPartialRefund = await _DbContext.Payments
               .Where(p => p.Type == PaymentType.PartialyRefunded)
               .SumAsync(p => p.RefundAmount).ConfigureAwait(false);

            return Ok(new MultiResult<int, int, int, decimal>(newOrderCount, openDisputeCount, openMessageCount, totalPrice - totalPartialRefund, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [MultiResultPropertyNames("lableList", "priceList", "countList")]
      [ProducesResponseType(typeof(MultiResult<List<string>, List<decimal>, List<int>>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> SalesStatistics(SalesPeriod salePeriod)
      {
         try
         {
            MultiResult<List<string>, List<decimal>, List<int>> result = new MultiResult<List<string>, List<decimal>, List<int>>();
            switch (salePeriod)
            {
               case SalesPeriod.Daily:
                  result = await GetDaily(CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext));
                  break;
               case SalesPeriod.Monthly:
                  result = await GetMonthly(CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext));
                  break;
               case SalesPeriod.Yearly:
                  result = await GetYearly(CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext));
                  break;
               default:
                  break;
            };
            return Ok(result);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetDaily(CustomAttributeTypedArgument customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         DateTime startDate = DateTime.UtcNow.AddMonths(-1).AddDays(1);
         for (DateTime date = startDate; date <= DateTime.UtcNow; date = date.AddDays(1))
         {
            lableList.Add(date.ToString("dd MMM"));
         }
         var totalList = await _DbContext.Orders
            .Where(o => o.Status == OrderStatusType.InProgress
                     || o.Status == OrderStatusType.Confirmed
                     || o.Status == OrderStatusType.Delivered
                     || o.Status == OrderStatusType.PartialyRefunded)
            .Where(o => o.Date > startDate)
            .OrderBy(o => o.Date)
                      .GroupBy(x =>
                             new
                             {
                                x.Date.Year,
                                x.Date.Month,
                                x.Date.Day
                             })
                      .Select(s => new
                      {
                         date = Convert.ToDateTime(new DateTime(s.Key.Year, s.Key.Month, s.Key.Day)).ToString("dd MMM"),
                         total = s.Sum(s => s.TotalPrice),
                         count = s.Count()
                      }).ToListAsync().ConfigureAwait(false);

         var refundList = await _DbContext.Payments
            .Where(o => o.Type == PaymentType.PartialyRefunded)
            .Where(o => o.DateTime > startDate)
            .OrderBy(o => o.DateTime)
                      .GroupBy(x =>
                             new
                             {
                                x.DateTime.Year,
                                x.DateTime.Month,
                                x.DateTime.Day
                             })
                      .Select(s => new
                      {
                         date = Convert.ToDateTime(new DateTime(s.Key.Year, s.Key.Month, s.Key.Day)).ToString("dd MMM"),
                         refund = s.Sum(s => s.RefundAmount)
                      }).ToListAsync().ConfigureAwait(false);

         foreach (var day in lableList)
         {
            if (totalList.Any(t => t.date == day))
            {
               if (refundList.Any(t => t.date == day))
                  priceList.Add(totalList.SingleOrDefault(t => t.date == day).total - refundList.SingleOrDefault(t => t.date == day).refund);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.date == day).total);

               countList.Add(totalList.SingleOrDefault(t => t.date == day).count);
            }
            else
            {
               priceList.Add(0);
               countList.Add(0);
            }
         }
         return new MultiResult<List<string>, List<decimal>, List<int>>(lableList, priceList, countList, customAttributeTypeArguments);
      }
      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetMonthly(CustomAttributeTypedArgument customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         DateTime startDate = DateTime.UtcNow.AddYears(-1).AddMonths(1);
         for (DateTime date = startDate; date <= DateTime.UtcNow; date = date.AddMonths(1))
         {
            lableList.Add(date.ToString("MMM"));
         }
         var totalList = await _DbContext.Orders
            .Where(o => o.Status == OrderStatusType.InProgress
                     || o.Status == OrderStatusType.Confirmed
                     || o.Status == OrderStatusType.Delivered
                     || o.Status == OrderStatusType.PartialyRefunded)
            .Where(o => o.Date > startDate)
            .OrderBy(o => o.Date)
                         .GroupBy(x =>
                                new
                                {
                                   x.Date.Year,
                                   x.Date.Month
                                })
                         .Select(s => new
                         {
                            date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key.Month),
                            total = s.Sum(s => s.TotalPrice),
                            count = s.Count()
                         }).ToListAsync().ConfigureAwait(false);


         var refundList = await _DbContext.Payments
            .Where(o => o.Type == PaymentType.PartialyRefunded)
            .Where(o => o.DateTime > startDate)
            .OrderBy(o => o.DateTime)
                      .GroupBy(x =>
                             new
                             {
                                x.DateTime.Year,
                                x.DateTime.Month
                             })
                      .Select(s => new
                      {
                         date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key.Month),
                         refund = s.Sum(s => s.RefundAmount)
                      }).ToListAsync().ConfigureAwait(false);

         foreach (var month in lableList)
         {
            if (totalList.Any(t => t.date == month))
            {

               if (refundList.Any(t => t.date == month))
                  priceList.Add(totalList.SingleOrDefault(t => t.date == month).total - refundList.SingleOrDefault(t => t.date == month).refund);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.date == month).total);
               countList.Add(totalList.SingleOrDefault(t => t.date == month).count);
            }
            else
            {
               priceList.Add(0);
               countList.Add(0);
            }
         }
         return new MultiResult<List<string>, List<decimal>, List<int>>(lableList, priceList, countList, customAttributeTypeArguments);
      }
      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetYearly(CustomAttributeTypedArgument customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         var totalList = await _DbContext.Orders
                       .Where(o => o.Status == OrderStatusType.InProgress
                                || o.Status == OrderStatusType.Confirmed
                                || o.Status == OrderStatusType.Delivered
                                || o.Status == OrderStatusType.PartialyRefunded)
                                  .OrderBy(o => o.Date)
                                  .GroupBy(x =>
                                         new
                                         {
                                            x.Date.Year
                                         })
                                  .Select(s => new
                                  {
                                     date = s.Key.Year.ToString(),
                                     total = s.Sum(s => s.TotalPrice),
                                     count = s.Count()
                                  }).ToListAsync().ConfigureAwait(false);

         var refundList = await _DbContext.Payments
          .Where(o => o.Type == PaymentType.PartialyRefunded)
          .OrderBy(o => o.DateTime)
                    .GroupBy(x =>
                         new
                         {
                            x.DateTime.Year
                         })
                    .Select(s => new
                    {
                       date = s.Key.Year.ToString(),
                       refund = s.Sum(s => s.RefundAmount)
                    }).ToListAsync().ConfigureAwait(false);

         lableList = totalList.Select(t => t.date).ToList();

         foreach (var year in lableList)
         {
            if (totalList.Any(t => t.date == year))
            {
               if (refundList.Any(t => t.date == year))
                  priceList.Add(totalList.SingleOrDefault(t => t.date == year).total - refundList.SingleOrDefault(t => t.date == year).refund);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.date == year).total);

               countList.Add(totalList.SingleOrDefault(t => t.date == year).count);
            }
            else
            {
               priceList.Add(0);
               countList.Add(0);
            }
         }
         return new MultiResult<List<string>, List<decimal>, List<int>>(lableList, priceList, countList, customAttributeTypeArguments);
      }


   }
}
