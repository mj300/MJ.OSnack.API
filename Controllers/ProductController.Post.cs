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
      [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
      #endregion
      [HttpPost("[action]")]
      [Authorize(AppConst.AccessPolicies.Secret)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status422UnprocessableEntity)]
      /// Done 
      public async Task<IActionResult> Post([FromBody] Product newProduct)
      {
         try
         {
            if (newProduct.Category.Id == 0)
               newProduct.Category = null;
            TryValidateModel(newProduct);
            ModelState.Remove("Category.Name");
            ModelState.Remove("Category.ImageBase64");
            ModelState.Remove("NutritionalInfo.Product");
            ModelState.Remove("Category.OriginalImageBase64");
            if (ModelState.ContainsKey("ImageBase64"))
               ModelState.Remove("OriginalImageBase64");

            if (!ModelState.IsValid)
            {
               CoreFunc.ExtractErrors(ModelState, ref ErrorsList);
               return UnprocessableEntity(ErrorsList);
            }

            /// check the database to see if a Product with the same name exists
            if (await _DbContext.Products.AnyAsync(d => d.Name == newProduct.Name && d.Category.Id == newProduct.Category.Id).ConfigureAwait(false))
            {
               CoreFunc.Error(ref ErrorsList, "Product already exists.");
               return StatusCode(412, ErrorsList);
            }

            try
            {
               string folderName = CoreFunc.StringGenerator(10, 3, 3, 4);
               newProduct.ImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                       _WebHost.WebRootPath,
                       newProduct.ImageBase64,
                       $"Images\\Products\\{folderName}");
               newProduct.OriginalImagePath = CoreFunc.SaveImageToWWWRoot(CoreFunc.StringGenerator(10, 3, 3, 4),
                       _WebHost.WebRootPath,
                       newProduct.OriginalImageBase64,
                       $"Images\\Products\\{folderName}");
            }
            catch (Exception ex)
            {
               CoreFunc.Error(ref ErrorsList, "Image cannot be saved.");
               _LoggingService.LogException(Request.Path, ex, User, AppLogType.FileModification);
               return StatusCode(412, ErrorsList);
            }

            try
            {
               await _DbContext.Products.AddAsync(newProduct).ConfigureAwait(false);
               _DbContext.Entry(newProduct.Category).State = EntityState.Unchanged;
               await _DbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
               CoreFunc.DeleteFromWWWRoot(newProduct.ImagePath, _WebHost.WebRootPath);
               CoreFunc.DeleteFromWWWRoot(newProduct.OriginalImagePath, _WebHost.WebRootPath);
               CoreFunc.ClearEmptyImageFolders(_WebHost.WebRootPath);
               throw;
            }

            return Created("", newProduct);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

   }
}
