using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Done 
      public async Task<IActionResult> Put([FromBody] Product modifiedProduct)
      {
         try
         {
            bool containsNewImages = true;
            if (modifiedProduct.Category.Id == 0)
               modifiedProduct.Category = null;

            ModelState.Remove("Category.Name");
            ModelState.Remove("Category.ImageBase64");
            ModelState.Remove("Category.OriginalImageBase64");
            ModelState.Remove("NutritionalInfo.Product");

            /// if new image is not provided do not check for new images
            if (string.IsNullOrWhiteSpace(modifiedProduct.ImageBase64) && string.IsNullOrWhiteSpace(modifiedProduct.OriginalImageBase64))
            {
               containsNewImages = false;
               ModelState.Remove("ImageBase64");
               ModelState.Remove("OriginalImageBase64");
            }

            if (ModelState.ContainsKey("ImageBase64"))
               ModelState.Remove("OriginalImageBase64");

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            /// check the database to see if a Product with the same name exists
            if (await _DbContext.Products.AnyAsync(d => d.Id != modifiedProduct.Id && d.Name == modifiedProduct.Name && d.Category.Id == modifiedProduct.Category.Id).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Duplicated product name in selected category.");
               return StatusCode(412, ErrorsList);
            }

            string oldImagePath = modifiedProduct.ImagePath.Clone().ToString();
            string oldOriginalImagePath = modifiedProduct.OriginalImagePath.Clone().ToString();
            /// if new image is provided save the new image
            if (containsNewImages)
            {
               try
               {
                  string folderName = CoreFunc.StringGenerator(10, 3, 3, 4);
                  modifiedProduct.ImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                          _WebHost.WebRootPath,
                          modifiedProduct.ImageBase64,
                          $"Images\\Products\\{folderName}");
                  modifiedProduct.OriginalImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                          _WebHost.WebRootPath,
                          modifiedProduct.OriginalImageBase64,
                          $"Images\\Products\\{folderName}");
               }
               catch (Exception ex)
               {
                  CoreFunc.Error(ref ErrorsList, "Image cannot be saved.");
                  _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
                  return StatusCode(412, ErrorsList);
               }
            }

            try
            {
               _DbContext.Products.Update(modifiedProduct);
               await _DbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
               if (containsNewImages)
               {
                  CoreFunc.DeleteFromWWWRoot(modifiedProduct.ImagePath, _WebHost.WebRootPath);
                  CoreFunc.DeleteFromWWWRoot(modifiedProduct.OriginalImagePath, _WebHost.WebRootPath);
                  CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
               }
               throw;
            }

            if (containsNewImages)
            {
               CoreFunc.DeleteFromWWWRoot(oldImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(oldOriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
            }

            return Ok(modifiedProduct);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
