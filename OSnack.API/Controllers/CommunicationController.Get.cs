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
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class CommunicationController
   {
      #region *** ***
      [MultiResultPropertyNames("communicationList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<Communication>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{selectedPage}/{maxNumberPerItemsPage}/{searchValue}/{isSortAsce}/{sortName}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> Search(
          int selectedPage,
          int maxNumberPerItemsPage,
          string searchValue = "",
          string filterStatus = CoreConst.GetAllRecords,
          bool isSortAsce = false,
          string sortName = "Date")
      {
         try
         {
            _ = bool.TryParse(filterStatus, out bool boolFilterStatus);

            int totalCount = await _unitOfWork.Communications
               .CountAsync(c => c.Type == ContactType.Message &&
               (filterStatus.Equals(CoreConst.GetAllRecords) || c.Status == boolFilterStatus) &&
               (searchValue.Equals(CoreConst.GetAllRecords) || c.Id.Contains(searchValue)
                                                              || c.FullName.Contains(searchValue)
                                                              || c.Email.Contains(searchValue)))
                .ConfigureAwait(false);

            List<Communication> list = await _unitOfWork.Communications
                .GetAllCommunication(c => c.Type == ContactType.Message &&
                         (filterStatus.Equals(CoreConst.GetAllRecords) || c.Status == boolFilterStatus) &&
                         (searchValue.Equals(CoreConst.GetAllRecords) || c.Id.Contains(searchValue)
                                                                       || c.FullName.Contains(searchValue)
                                                                       || c.Email.Contains(searchValue))
                , isSortAsce, sortName, selectedPage, maxNumberPerItemsPage)
                .ConfigureAwait(false);

            return Ok(new MultiResult<List<Communication>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
      #region *** ***
      [ProducesResponseType(typeof(Communication), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpGet("Get/[action]/{questionKey}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> GetQuestion(string questionKey)
      {
         try
         {

            Communication question = await _unitOfWork.Communications
               .GetCommunicationWithMessages(questionKey)
               .ConfigureAwait(false);

            if (question is null)
            {
               CoreFunc.Error(ref ErrorsList, "Communication Not Found.");
               return StatusCode(412, ErrorsList);
            }

            return Ok(question);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
      #region *** ***
      [ProducesResponseType(typeof(Communication), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpGet("Get/[action]/{disputeKey}")]
      [Authorize(AppConst.AccessPolicies.Official)]
      public async Task<IActionResult> GetDispute(string disputeKey)
      {
         try
         {

            Communication dispute = await _unitOfWork.Communications
              .GetDisputeWithMessages(AppFunc.GetUserId(User), disputeKey)
               .ConfigureAwait(false);

            if (dispute is null)
            {
               CoreFunc.Error(ref ErrorsList, "Dispute Not Found.");
               return StatusCode(412, ErrorsList);
            }

            return Ok(dispute);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
      #region *** ***
      [ProducesResponseType(typeof(Communication), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpGet("Get/[action]/{disputeKey}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> GetDisputeSecret(string disputeKey)
      {
         try
         {

            Communication dispute = await _unitOfWork.Communications
               .GetDisputeWithMessages(disputeKey)
               .ConfigureAwait(false);

            if (dispute is null)
            {
               CoreFunc.Error(ref ErrorsList, "Dispute Not Found.");
               return StatusCode(412, ErrorsList);
            }

            return Ok(dispute);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }


   }
}
