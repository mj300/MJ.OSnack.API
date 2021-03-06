using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class OrderController
   {
      /// <summary>
      /// Used to get a list of all Order with OrderItems
      /// </summary>
      #region *** 200 OK, 417 ExpectationFailed ***
      [Consumes(MediaTypeNames.Application.Json)]
      [MultiResultPropertyNames(new string[] { "orderList", "availableTypes", "totalCount", "disputeFilterType" })]
      [ProducesResponseType(typeof(MultiResult<List<Order>, List<OrderStatusType>, int, DisputeFilterTypes>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{selectedPage}/{maxNumberPerItemsPage}/{searchValue}/{filterStatus}/{isSortAsce}/{sortName}/{disputeFilter}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> All(
          int selectedPage,
          int maxNumberPerItemsPage,
          string searchValue = "",
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = true,
          string sortName = "Date",
          string disputeFilter = CoreConst.GetAllRecords)
      {
         try
         {

            _ = bool.TryParse(disputeFilter, out bool boolDisputeFilter);


            bool closeDisput = await _unitOfWork.Orders.HasDispute(false).ConfigureAwait(false);
            bool openDisput = await _unitOfWork.Orders.HasDispute(true).ConfigureAwait(false);


            int totalCount = await _unitOfWork.Orders
               .OrderCountAsync(o =>
                  (filterStatus.Equals(CoreConst.GetAllRecords) || o.Status.Equals((OrderStatusType)Enum.Parse(typeof(OrderStatusType), filterStatus, true))) &&
                  (disputeFilter.Equals(CoreConst.GetAllRecords) || o.Dispute.Status == boolDisputeFilter) &&
                  (searchValue.Equals(CoreConst.GetAllRecords) || o.Name.Contains(searchValue)
                                             || o.User.FirstName.Contains(searchValue)
                                             || o.User.Surname.Contains(searchValue)
                                             || o.User.Email.Contains(searchValue)
                                             || o.Id.Contains(searchValue)
                                             || o.Postcode.Contains(searchValue)
                                             || o.Payment.Email.Contains(searchValue)
                                             || o.Payment.Reference.Contains(searchValue)))
               .ConfigureAwait(false);

            List<OrderStatusType> availebeStatusTypes = await _unitOfWork.Orders
                        .AvailebeStatusTypes().ConfigureAwait(false);

            List<Order> list = await _unitOfWork.Orders
               .GetOrderByPageAsync(o =>
                  (filterStatus.Equals(CoreConst.GetAllRecords) || o.Status.Equals((OrderStatusType)Enum.Parse(typeof(OrderStatusType), filterStatus, true))) &&
                  (disputeFilter.Equals(CoreConst.GetAllRecords) || o.Dispute.Status == boolDisputeFilter) &&
                  (searchValue.Equals(CoreConst.GetAllRecords) || o.Name.Contains(searchValue)
                                             || o.User.FirstName.Contains(searchValue)
                                             || o.User.Surname.Contains(searchValue)
                                             || o.User.Email.Contains(searchValue)
                                             || o.Id.Contains(searchValue)
                                             || o.Postcode.Contains(searchValue)
                                             || o.Payment.Email.Contains(searchValue)
                                             || o.Payment.Reference.Contains(searchValue))
                                             , isSortAsce, sortName, selectedPage, maxNumberPerItemsPage)
                .ConfigureAwait(false);

            return Ok(new MultiResult<List<Order>, List<OrderStatusType>, int, DisputeFilterTypes>(list, availebeStatusTypes, totalCount,
              AppFunc.GetDisputeFilterTypes(closeDisput, openDisput),
               CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      /// <summary>
      /// Used to get a list of all Order with OrderItems
      /// </summary>
      #region ***  ***
      [Consumes(MediaTypeNames.Application.Json)]
      [MultiResultPropertyNames("orderList", "availableTypes", "fullName", "totalCount", "disputeFilterType")]
      [ProducesResponseType(typeof(MultiResult<List<Order>, List<OrderStatusType>, string, int, DisputeFilterTypes>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{userId}/{selectedPage}/{maxNumberPerItemsPage}/{filterStatus}/{disputeFilter}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> AllUser(int userId,
          int selectedPage,
          int maxNumberPerItemsPage,
            string searchValue = "",
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = true,
          string sortName = "Date",
          string disputeFilter = CoreConst.GetAllRecords) =>
        await AllOrder(userId, selectedPage, maxNumberPerItemsPage, searchValue, filterStatus, isSortAsce, sortName, disputeFilter).ConfigureAwait(false);

      /// <summary>
      /// Used to get a list of all Order with OrderItems
      /// </summary>
      #region *** 200 OK, 417 ExpectationFailed ***
      [Consumes(MediaTypeNames.Application.Json)]
      [MultiResultPropertyNames("orderList", "availableTypes", "fullName", "totalCount", "disputeFilterType")]
      [ProducesResponseType(typeof(MultiResult<List<Order>, List<OrderStatusType>, string, int, DisputeFilterTypes>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{selectedPage}/{maxNumberPerItemsPage}/{filterStatus}/{disputeFilter}")]
      [Authorize(AppConst.AccessPolicies.Official)]
      public async Task<IActionResult> AllOfficial(int selectedPage,
          int maxNumberPerItemsPage,
           string searchValue = "",
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = false,
          string sortName = "Date",
          string disputeFilter = CoreConst.GetAllRecords) =>
        await AllOrder(AppFunc.GetUserId(User), selectedPage, maxNumberPerItemsPage, searchValue, filterStatus, isSortAsce, sortName, disputeFilter).ConfigureAwait(false);

      private async Task<IActionResult> AllOrder(int userId, int selectedPage,
          int maxNumberPerItemsPage,
           string searchValue = "",
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = false,
          string sortName = "Date",
          string disputeFilter = CoreConst.GetAllRecords)
      {
         try
         {

            _ = bool.TryParse(disputeFilter, out bool boolDisputeFilter);

            bool closeDisput = await _unitOfWork.Orders.HasDispute(userId, false).ConfigureAwait(false);
            bool openDisput = await _unitOfWork.Orders.HasDispute(userId, true).ConfigureAwait(false);

            int totalCount = await _unitOfWork.Orders
               .OrderCountAsync(o => o.User.Id == userId &&
               (filterStatus.Equals(CoreConst.GetAllRecords) || o.Status.Equals((OrderStatusType)Enum.Parse(typeof(OrderStatusType), filterStatus, true))) &&
               (disputeFilter.Equals(CoreConst.GetAllRecords) || o.Dispute.Status == boolDisputeFilter) &&
               (searchValue.Equals(CoreConst.GetAllRecords) || (o.Id.Contains(searchValue)
                                                            || o.Postcode.Contains(searchValue)
                                                            || o.Payment.Email.Contains(searchValue)
                                                            || o.Payment.Reference.Contains(searchValue))))
                .ConfigureAwait(false);

            List<OrderStatusType> availebeStatusTypes = await _unitOfWork.Orders
                        .AvailebeStatusTypes(userId).ConfigureAwait(false);

            List<Order> list = await _unitOfWork.Orders
               .GetOrderByPageAsync(o => o.User.Id == userId &&
               (filterStatus.Equals(CoreConst.GetAllRecords) || o.Status.Equals((OrderStatusType)Enum.Parse(typeof(OrderStatusType), filterStatus, true))) &&
               (disputeFilter.Equals(CoreConst.GetAllRecords) || o.Dispute.Status == boolDisputeFilter) &&
               (searchValue.Equals(CoreConst.GetAllRecords) || (o.Id.Contains(searchValue)
                                                            || o.Postcode.Contains(searchValue)
                                                            || o.Payment.Email.Contains(searchValue)
                                                            || o.Payment.Reference.Contains(searchValue)))
                       , isSortAsce, sortName, selectedPage, maxNumberPerItemsPage)
                .ConfigureAwait(false);


            User user = await _unitOfWork.Users.FindAsync(userId);

            return Ok(new MultiResult<List<Order>, List<OrderStatusType>, string, int, DisputeFilterTypes>
               (list, availebeStatusTypes, $"{user.FirstName} {user.Surname}", totalCount,
               AppFunc.GetDisputeFilterTypes(closeDisput, openDisput),
               CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
