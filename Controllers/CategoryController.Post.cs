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
      [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test
      [HttpPost("[action]")]
      public async Task<IActionResult> Post([FromBody] Category newCategory)
      {
         try
         {
            TryValidateModel(newCategory);

            if (ModelState.ContainsKey("ImageBase64"))
               ModelState.Remove("OriginalImageBase64");

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            /// check the database to see if a Category with the same name exists
            if (await _DbContext.Categories
                .AnyAsync(d => d.Name.Equals(newCategory.Name)).ConfigureAwait(false))
            {
               /// extract the errors and return bad request containing the errors
               CoreFunc.Error(ref ErrorsList, "Category already exists.");
               return StatusCode(412, ErrorsList);
            }

            try
            {
               string folderName = CoreFunc.StringGenerator(10, 3, 3, 4);
               newCategory.ImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                       _WebHost.WebRootPath,
                       newCategory.ImageBase64,
                       $"Images\\Categories\\{folderName}");
               newCategory.OriginalImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                       _WebHost.WebRootPath,
                       newCategory.OriginalImageBase64,
                       $"Images\\Categories\\{folderName}");
            }
            catch (Exception ex)
            {
               CoreFunc.Error(ref ErrorsList, "Image cannot be saved.");
               _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
               return StatusCode(412, ErrorsList);
            }
            try
            {
               /// else Category object is made without any errors
               /// Add the new Category to the EF context
               await _DbContext.Categories.AddAsync(newCategory).ConfigureAwait(false);
               /// save the changes to the database
               await _DbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
               CoreFunc.DeleteFromWWWRoot(newCategory.ImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(newCategory.OriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
               CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification));
            }

            /// return 201 created status with the new object
            /// and success message
            return Created("", newCategory);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
