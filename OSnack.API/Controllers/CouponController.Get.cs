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
   public partial class CouponController
   {
      /// <summary>
      /// Search or get all the coupon
      /// search by Code or filter by type
      /// </summary>
      #region *** ***
      [MultiResultPropertyNames("couponList", "totalCount")]
      [ProducesResponseType(typeof(MultiResult<List<Coupon>, int>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<P8B.Core.CSharp.Models.Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{selectedPage}/{maxNumberPerItemsPage}/{searchValue}/{filterType}/{isSortAsce}/{sortName}")]
      [Authorize(AppConst.AccessPolicies.Secret)] /// Ready for test 
      public async Task<IActionResult> Search(
          int selectedPage,
          int maxNumberPerItemsPage,
          string searchValue = "",
          string filterType = CoreConst.GetAllRecords,
          bool isSortAsce = true,
          string sortName = "Code")
      {
         try
         {
            int totalCount = await _unitOfWork.Coupons.CountAsync(c =>
                (filterType.Equals(CoreConst.GetAllRecords) || c.Type.Equals((CouponType)Enum.Parse(typeof(CouponType), filterType, true))) &&
                (searchValue.Equals(CoreConst.GetAllRecords) || c.Code.Contains(searchValue)))
                .ConfigureAwait(false);


            List<Coupon> list = await _unitOfWork.Coupons
               .GetByPageAsync(c =>
               (filterType.Equals(CoreConst.GetAllRecords) || c.Type.Equals((CouponType)Enum.Parse(typeof(CouponType), filterType, true))) &&
                (searchValue.Equals(CoreConst.GetAllRecords) || c.Code.Contains(searchValue)),
                isSortAsce, sortName, selectedPage, maxNumberPerItemsPage).ConfigureAwait(false);

            return Ok(new MultiResult<List<Coupon>, int>(list, totalCount, CoreFunc.GetCustomAttributeTypedArgument(this.ControllerContext)));
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }

      /// <summary>
      /// Check Coupon code validation
      /// </summary>
      #region *** ***
      [ProducesResponseType(typeof(Coupon), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status412PreconditionFailed)]
      [ProducesResponseType(typeof(List<Error>), StatusCodes.Status417ExpectationFailed)]
      #endregion
      [HttpGet("Get/[action]/{couponCode?}")]
      [Authorize(AppConst.AccessPolicies.Public)]
      public async Task<IActionResult> Validate(string couponCode)
      {
         try
         {
            Coupon coupon = await _unitOfWork.Coupons.FindAsync(couponCode);

            if (coupon == null)
            {
               CoreFunc.Error(ref ErrorsList, $"'{couponCode}' not found");
               return StatusCode(412, ErrorsList);
            }

            if (coupon.MaxUseQuantity == 0 || coupon.ExpiryDate < DateTime.UtcNow)
            {
               CoreFunc.Error(ref ErrorsList, $"'{couponCode}' has expired");
               return StatusCode(412, ErrorsList);
            }

            return Ok(coupon);
         }
         catch (Exception ex)
         {
            CoreFunc.Error(ref ErrorsList, _LoggingService.LogException(Request.Path, ex, User));
            return StatusCode(417, ErrorsList);
         }
      }
   }
}
