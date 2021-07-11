using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp;
using P8B.Core.CSharp.Attributes;
using P8B.Core.CSharp.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class CommentController
   {

      #region *** 200 OK, 417 ExpectationFailed ***
      [MultiResultPropertyNames(new string[] { "commentList", "comment", "totalCount" })]
      [ProducesResponseType(typeof(MultiResult<List<Comment>, Comment, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("[action]/{productId}/{selectedPage}/{maxItemsPerPage}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> Get(int productId,
          int selectedPage = 1,
          int maxItemsPerPage = 5)
      {
         try
         {
            if (selectedPage == 0) selectedPage = 1;
            if (maxItemsPerPage == 0) maxItemsPerPage = 5;

            int totalCount = await _unitOfWork.Comments.CountAsync(c => c.Product.Id == productId)
                              .ConfigureAwait(false);


            List<Comment> list = await _unitOfWork.Comments
               .GetListOfCommentByProductId(productId, selectedPage, maxItemsPerPage)
               .ConfigureAwait(false);

            Comment selectComment = null;
            if (AppFunc.GetUserId(User) != 0)
            {
               selectComment = await _unitOfWork.Comments
                           .GetSelectedComment(AppFunc.GetUserId(User), productId).ConfigureAwait(false);
            }
            return Ok(new MultiResult<List<Comment>, Comment, int>(list, selectComment, totalCount, CoreFunc.GetCustomAttributeTypedArgument(ControllerContext)));
         }
         catch (Exception ex)
         {
            /// in the case any exceptions return the following error
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** 200 OK, 417 ExpectationFailed ***
      [MultiResultPropertyNames(new string[] { "commentList", "totalCount" })]
      [ProducesResponseType(typeof(MultiResult<List<Comment>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{productId}/{selectedPage}/{maxItemsPerPage}")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      public async Task<IActionResult> All(int productId,
          int selectedPage,
          int maxItemsPerPage)
      {
         try
         {
            int totalCount = await _unitOfWork.Comments.CountAsync(c => c.Product.Id == productId)
                                    .ConfigureAwait(false);


            List<Comment> list = await _unitOfWork.Comments
               .GetListOfCommentByProductId(productId, selectedPage, maxItemsPerPage)
               .ConfigureAwait(false);

            return Ok(new MultiResult<List<Comment>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(ControllerContext)));
         }
         catch (Exception ex)
         {
            /// in the case any exceptions return the following error
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
