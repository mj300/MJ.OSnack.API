using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OSnack.API.Database.Models;
using OSnack.API.Extras;
using P8B.Core.CSharp;
using P8B.Core.CSharp.Models;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class CommentController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]

      #endregion
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test
      [HttpPut("Put/[action]")]
      public async Task<IActionResult> AddReply([FromBody] Comment modifiedComment)
      {

         try
         {
            ModelState.Clear();
            TryValidateModel(modifiedComment);

            foreach (var key in ModelState.Keys)
            {
               if (key.StartsWith("User") || key.StartsWith("Product"))
                  ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            Comment comment = await _unitOfWork.Comments
               .FindAsync(modifiedComment.Id).ConfigureAwait(false);
            if (comment == null)
            {
               CoreFunc.Error(ref ErrorsList, "Comment not exist.");
               return StatusCode(412, ErrorsList);
            }
            comment.Reply = modifiedComment.Reply;

            _unitOfWork.Comments.Modify(comment);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            return Ok(comment);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]

      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test
      [HttpPut("[action]")]
      public async Task<IActionResult> Put([FromBody] Comment modifiedComment)
      {

         try
         {
            ModelState.Clear();
            TryValidateModel(modifiedComment);

            foreach (var key in ModelState.Keys)
            {
               if (key.StartsWith("User") || key.StartsWith("Product"))
                  ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            Comment comment = await _unitOfWork.Comments
               .GetSelectedComment(AppFunc.GetUserId(User), modifiedComment.Product.Id).ConfigureAwait(false);
            if (comment == null)
            {
               CoreFunc.Error(ref ErrorsList, "Comment not exist.");
               return StatusCode(412, ErrorsList);
            }

            comment.Description = modifiedComment.Description;
            comment.Rate = modifiedComment.Rate;
            await comment.CencoredDescription();
            _unitOfWork.Comments.Modify(comment);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            return Ok(comment);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
