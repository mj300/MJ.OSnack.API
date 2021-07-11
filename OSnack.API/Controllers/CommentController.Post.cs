using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
   public partial class CommentController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Comment), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Official)]  /// Ready For Test
      [HttpPost("[action]")]
      public async Task<IActionResult> Post([FromBody] Comment newComment)
      {

         try
         {

            User user = await _unitOfWork.Users.GetUserWithItsOrder(AppFunc.GetUserId(User))
               .ConfigureAwait(false);

            if (user.Orders.Any(o => o.Status == OrderStatusType.Delivered &&
                                     o.OrderItems.Any(oi => oi.ProductId == newComment.Product.Id)))
            {
               Comment selectComment = await _unitOfWork.Comments
                  .GetSelectedComment(AppFunc.GetUserId(User), newComment.Product.Id)
                  .ConfigureAwait(false);

               if (selectComment != null)
               {
                  _LoggingService.Log(Request.Path, AppLogType.OrderException,
                  new { message = $"Try to add duplicate comment.", newContact = newComment }, User);
                  CoreFunc.Error(ref ErrorsList, "You Can't add review for this product.Try again or Contact Admin.");
                  return StatusCode(412, ErrorsList);
               }
            }
            else
            {
               _LoggingService.Log(Request.Path, AppLogType.OrderException,
                   new { message = $"Try to add comment without order.", newContact = newComment }, User);
               CoreFunc.Error(ref ErrorsList, "You Can't add review for this product.Try again or Contact Admin.");
               return StatusCode(412, ErrorsList);
            }

            newComment.User = user;

            newComment.User.Orders = null;

            ModelState.Clear();
            TryValidateModel(newComment);
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

            await _unitOfWork.Comments.AddComment(newComment);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            return Created("", newComment);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
