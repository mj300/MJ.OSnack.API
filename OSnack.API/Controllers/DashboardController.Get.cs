using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
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
            int newOrderCount = await _unitOfWork.Orders
               .CountAsync(o => o.Status == OrderStatusType.InProgress).ConfigureAwait(false);

            int openDisputeCount = await _unitOfWork.Communications
               .CountAsync(o => o.Type == ContactType.Dispute && o.Status == true).ConfigureAwait(false);

            int openMessageCount = await _unitOfWork.Communications
               .CountAsync(o => o.Type == ContactType.Message && o.Status == true).ConfigureAwait(false);

            decimal totalPrice = await _unitOfWork.Dashboard
               .TotalPriceOrders().ConfigureAwait(false);

            decimal totalPartialRefund = await _unitOfWork.Dashboard
              .TotalPartialRefund().ConfigureAwait(false);

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


      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetDaily(CustomAttributeTypedArgument? customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         DateTime startDate = DateTime.UtcNow.AddMonths(-1).AddDays(1);
         for (DateTime date = startDate; date <= DateTime.UtcNow; date = date.AddDays(1))
         {
            lableList.Add(date.ToString("dd MMM"));
         }
         var totalList = await _unitOfWork.Dashboard
            .GetDailyTotalSaleFromDay(startDate).ConfigureAwait(false);

         var refundList = await _unitOfWork.Dashboard
           .GetDailyTotalPartialyRefundedFromDay(startDate).ConfigureAwait(false);

         foreach (var day in lableList)
         {
            if (totalList.Any(t => t.Date == day))
            {
               if (refundList.Any(t => t.Date == day))
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == day).Total - refundList.SingleOrDefault(t => t.Date == day).Total);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == day).Total);

               countList.Add(totalList.SingleOrDefault(t => t.Date == day).Count);
            }
            else
            {
               priceList.Add(0);
               countList.Add(0);
            }
         }
         return new MultiResult<List<string>, List<decimal>, List<int>>(lableList, priceList, countList, customAttributeTypeArguments);
      }
      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetMonthly(CustomAttributeTypedArgument? customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         DateTime startDate = DateTime.UtcNow.AddYears(-1).AddMonths(1);
         for (DateTime date = startDate; date <= DateTime.UtcNow; date = date.AddMonths(1))
         {
            lableList.Add(date.ToString("MMM"));
         }

         var totalList = await _unitOfWork.Dashboard
            .GetMonthlyTotalSaleFromDay(startDate).ConfigureAwait(false);


         var refundList = await _unitOfWork.Dashboard
            .GetMonthlyTotalPartialyRefundedFromDay(startDate).ConfigureAwait(false);

         foreach (var month in lableList)
         {
            if (totalList.Any(t => t.Date == month))
            {

               if (refundList.Any(t => t.Date == month))
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == month).Total - refundList.SingleOrDefault(t => t.Date == month).Total);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == month).Total);
               countList.Add(totalList.SingleOrDefault(t => t.Date == month).Count);
            }
            else
            {
               priceList.Add(0);
               countList.Add(0);
            }
         }
         return new MultiResult<List<string>, List<decimal>, List<int>>(lableList, priceList, countList, customAttributeTypeArguments);
      }
      private async Task<MultiResult<List<string>, List<decimal>, List<int>>> GetYearly(CustomAttributeTypedArgument? customAttributeTypeArguments)
      {
         List<string> lableList = new List<string>();
         List<decimal> priceList = new List<decimal>();
         List<int> countList = new List<int>();
         var totalList = await _unitOfWork.Dashboard
            .GetYearlyTotalSaleFromDay().ConfigureAwait(false);

         var refundList = await _unitOfWork.Dashboard
            .GetYearlyTotalPartialyRefundedFromDay().ConfigureAwait(false);

         lableList = totalList.Select(t => t.Date).ToList();

         foreach (var year in lableList)
         {
            if (totalList.Any(t => t.Date == year))
            {
               if (refundList.Any(t => t.Date == year))
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == year).Total - refundList.SingleOrDefault(t => t.Date == year).Total);
               else
                  priceList.Add(totalList.SingleOrDefault(t => t.Date == year).Total);

               countList.Add(totalList.SingleOrDefault(t => t.Date == year).Count);
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
