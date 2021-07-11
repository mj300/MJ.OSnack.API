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
using System.Net.Mime;
using System.Threading.Tasks;

namespace OSnack.API.Controllers
{
   public partial class ProductController
   {
      #region ***  ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpDelete("[action]/{productId}")]
      [Authorize(AppConst.AccessPolicies.Secret)]

      /// Done    
      public async Task<IActionResult> Delete(int productId)
      {
         try
         {
            Product product = await _unitOfWork.Products.FindAsync(productId).ConfigureAwait(false);
            if (product is null)
            {
               CoreFunc.Error(ref ErrorsList, "Product not found");
               return NotFound(ErrorsList);
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            try
            {
               CoreFunc.DeleteFromWWWRoot(product.ImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(product.OriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
            }
            catch (Exception ex)
            {
               _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
            }
            return Ok($"Product '{product.Name}' was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
