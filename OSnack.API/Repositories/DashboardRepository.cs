using Microsoft.EntityFrameworkCore;
using OSnack.API.Database;
using OSnack.API.Extras.CustomTypes;
using OSnack.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OSnack.API.Repositories
{
   public class DashboardRepository : IDashboardRepository
   {
      protected readonly OSnackDbContext _DbContext;
      public DashboardRepository(OSnackDbContext context)
      {
         _DbContext = context;
      }

      public async Task<decimal> TotalPriceOrders()
      {
         return await _DbContext.Orders
                .Where(o => o.Status == OrderStatusType.InProgress
                     || o.Status == OrderStatusType.Confirmed
                     || o.Status == OrderStatusType.Delivered
                     || o.Status == OrderStatusType.PartialyRefunded)
                .SumAsync(o => o.TotalPrice).ConfigureAwait(false);
      }

      public async Task<decimal> TotalPartialRefund()
      {
         return await _DbContext.Payments
                .Where(p => p.Type == PaymentType.PartialyRefunded)
                .SumAsync(p => p.RefundAmount).ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetDailyTotalSaleFromDay(DateTime startDate)
      {
         return await _DbContext.Orders
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
                      .Select(s => new TotalSalesPeriod
                      {
                         Date = Convert.ToDateTime(new DateTime(s.Key.Year, s.Key.Month, s.Key.Day)).ToString("dd MMM"),
                         Total = s.Sum(s => s.TotalPrice),
                         Count = s.Count()
                      }).ToListAsync().ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetDailyTotalPartialyRefundedFromDay(DateTime startDate)
      {
         return await _DbContext.Payments
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
                       .Select(s => new TotalSalesPeriod
                       {
                          Date = Convert.ToDateTime(new DateTime(s.Key.Year, s.Key.Month, s.Key.Day)).ToString("dd MMM"),
                          Total = s.Sum(s => s.RefundAmount)
                       }).ToListAsync().ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetMonthlyTotalSaleFromDay(DateTime startDate)
      {
         return await _DbContext.Orders
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
                         .Select(s => new TotalSalesPeriod
                         {
                            Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key.Month),
                            Total = s.Sum(s => s.TotalPrice),
                            Count = s.Count()
                         }).ToListAsync().ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetMonthlyTotalPartialyRefundedFromDay(DateTime startDate)
      {
         return await _DbContext.Payments
            .Where(o => o.Type == PaymentType.PartialyRefunded)
            .Where(o => o.DateTime > startDate)
            .OrderBy(o => o.DateTime)
                      .GroupBy(x =>
                             new
                             {
                                x.DateTime.Year,
                                x.DateTime.Month
                             })
                      .Select(s => new TotalSalesPeriod
                      {
                         Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key.Month),
                         Total = s.Sum(s => s.RefundAmount)
                      }).ToListAsync().ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetYearlyTotalSaleFromDay()
      {
         return await _DbContext.Orders
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
                                  .Select(s => new TotalSalesPeriod
                                  {
                                     Date = s.Key.Year.ToString(),
                                     Total = s.Sum(s => s.TotalPrice),
                                     Count = s.Count()
                                  }).ToListAsync().ConfigureAwait(false);
      }

      public async Task<List<TotalSalesPeriod>> GetYearlyTotalPartialyRefundedFromDay()
      {
         return await _DbContext.Payments
          .Where(o => o.Type == PaymentType.PartialyRefunded)
          .OrderBy(o => o.DateTime)
                    .GroupBy(x =>
                         new
                         {
                            x.DateTime.Year
                         })
                    .Select(s => new TotalSalesPeriod
                    {
                       Date = s.Key.Year.ToString(),
                       Total = s.Sum(s => s.RefundAmount)
                    }).ToListAsync().ConfigureAwait(false);
      }
   }
}
