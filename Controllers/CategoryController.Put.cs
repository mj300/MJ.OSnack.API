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
   public partial class CategoryController
   {
      #region *** ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpPut("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Done   
      public async Task<IActionResult> Put([FromBody] Category modifiedCategory)
      {
         try
         {
            bool containsNewImages = true;
            TryValidateModel(modifiedCategory);

            /// if new image is not provided do not check for new images
            if (string.IsNullOrWhiteSpace(modifiedCategory.ImageBase64) && string.IsNullOrWhiteSpace(modifiedCategory.OriginalImageBase64))
            {
               containsNewImages = false;
               ModelState.Remove("ImageBase64");
               ModelState.Remove("OriginalImageBase64");
            }

            if (ModelState.ContainsKey("ImageBase64"))
               ModelState.Remove("OriginalImageBase64");
            /// if model validation failed
            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               /// return Unprocessable Entity with all the errors
               return UnprocessableEntity(ErrorsList);
            }

            /// check the database to see if a Category with the same name exists
            if (await _DbContext.Categories
                .AnyAsync(c => c.Name == modifiedCategory.Name && c.Id != modifiedCategory.Id)
                .ConfigureAwait(false))
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Category with the given name already exists.");
               return StatusCode(412, ErrorsList);
            }

            /// get the current category
            Category currentCatogory = await _DbContext.Categories
                .SingleOrDefaultAsync(c => c.Id == modifiedCategory.Id)
                .ConfigureAwait(false);

            // if the current category does not exists
            if (currentCatogory == null)
            {
               CoreFunc.Error(ref ErrorsList, "Category Not Found");
               return NotFound(ErrorsList);
            }

            string oldImagePath = modifiedCategory.ImagePath.Clone().ToString();
            string oldOriginalImagePath = modifiedCategory.OriginalImagePath.Clone().ToString();

            /// if new image is provided save the new image
            if (containsNewImages)
            {
               try
               {
                  string folderName = CoreFunc.StringGenerator(10, 3, 3, 4);
                  modifiedCategory.ImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                          _WebHost.WebRootPath,
                          modifiedCategory.ImageBase64,
                          $"Images\\Categories\\{folderName}");
                  modifiedCategory.OriginalImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                          _WebHost.WebRootPath,
                          modifiedCategory.OriginalImageBase64,
                          $"Images\\Categories\\{folderName}");
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
               /// else Category object is made without any errors
               _DbContext.Categories.Update(modifiedCategory);

               await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            }
            catch (Exception ex)
            {
               if (containsNewImages)
               {
                  CoreFunc.DeleteFromWWWRoot(modifiedCategory.ImagePath, _WebHost.WebRootPath);
                  CoreFunc.DeleteFromWWWRoot(modifiedCategory.OriginalImagePath, _WebHost.WebRootPath);
                  CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
               }
               _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
            }

            if (containsNewImages)
            {
               CoreFunc.DeleteFromWWWRoot(oldImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(oldOriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
            }
            return Ok(modifiedCategory);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
