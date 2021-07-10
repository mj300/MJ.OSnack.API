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
      #region ***  ***
      [Consumes(MediaTypeNames.Application.Json)]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      #endregion
      [HttpDelete("[action]/{categoryId}")]
      [Authorize(AppConst.AccessPolicies.Secret)]  /// Ready For Test 
      public async Task<IActionResult> Delete(int categoryId)
      {
         try
         {
            Category category = await _DbContext.Categories
                 .SingleOrDefaultAsync(c => c.Id == categoryId)
                 .ConfigureAwait(false);
            if (category is null)
            {
               CoreFunc.Error(ref ErrorsList, "Category not found");
               return NotFound(ErrorsList);
            }

            /// If the category is in use by any product then do not allow delete
            if (await _DbContext.Products
                .AnyAsync(c => c.Category.Id == category.Id)
                .ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Unable to delete. Category is in use by at least one product");
               return StatusCode(412, ErrorsList);
            }

            _DbContext.Categories.Remove(category);
            await _DbContext.SaveChangesAsync().ConfigureAwait(false);

            try
            {
               CoreFunc.DeleteFromWWWRoot(category.ImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(category.OriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
            }
            catch (Exception ex)
            {
               _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
            }
            return Ok($"Category '{category.Name}' was deleted");
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
