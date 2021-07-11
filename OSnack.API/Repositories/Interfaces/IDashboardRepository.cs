using OSnack.API.Extras.CustomTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Repositories.Interfaces
{
   public interface IDashboardRepository
   {
      Task<decimal> TotalPriceOrders();
      Task<decimal> TotalPartialRefund();
      Task<List<TotalSalesPeriod>> GetDailyTotalSaleFromDay(DateTime startDate);
      Task<List<TotalSalesPeriod>> GetDailyTotalPartialyRefundedFromDay(DateTime startDate);
      Task<List<TotalSalesPeriod>> GetMonthlyTotalSaleFromDay(DateTime startDate);
      Task<List<TotalSalesPeriod>> GetMonthlyTotalPartialyRefundedFromDay(DateTime startDate);
      Task<List<TotalSalesPeriod>> GetYearlyTotalSaleFromDay();
      Task<List<TotalSalesPeriod>> GetYearlyTotalPartialyRefundedFromDay();
   }
}
