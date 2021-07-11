using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Moq;
using OSnack.API.Controllers;
using OSnack.API.Database;
using OSnack.API.Database.Models;
using P8B.Core.CSharp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OSnack.API.test
{
   public class CouponTest : IDisposable
   {

      protected readonly OSnackDbContext _context;
      readonly CouponController couponController;

      public CouponTest()
      {
         var options = new DbContextOptionsBuilder<OSnackDbContext>()
         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
         .Options;


         var coupons = new[]
         {
            new Coupon { Code = "Free1", DiscountAmount = 0, ExpiryDate = DateTime.Now.AddDays(-2), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.FreeDelivery },
            new Coupon { Code = "10Percent", DiscountAmount = 10, ExpiryDate = DateTime.Now.AddDays(20), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.PercentageOfTotal },
            new Coupon { Code = "5Pound", DiscountAmount = 0, ExpiryDate = DateTime.Now.AddDays(20), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.DiscountPrice },
            new Coupon { Code = "10Pound", DiscountAmount = 0, ExpiryDate = DateTime.Now.AddDays(20), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.DiscountPrice },
            new Coupon { Code = "Free2", DiscountAmount = 0, ExpiryDate = DateTime.Now.AddDays(20), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.FreeDelivery },
            new Coupon { Code = "Free3", DiscountAmount = 0, ExpiryDate = DateTime.Now.AddDays(20), MaxUseQuantity = 5, MinimumOrderPrice = 15, Type = Extras.CustomTypes.CouponType.FreeDelivery },

         };

         _context = new OSnackDbContext(options);

         _context.Database.EnsureCreated();
         _context.Coupons.AddRange(coupons);
         _context.SaveChanges();

         couponController = new CouponController(_context);
         var objectValidator = new Mock<IObjectModelValidator>();
         objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                           It.IsAny<ValidationStateDictionary>(),
                                           It.IsAny<string>(),
                                           It.IsAny<Object>()));
         couponController.ObjectValidator = objectValidator.Object;
      }

      [Fact]
      public async Task GetCouponPageTwoTakeTow()
      {

         var result = await couponController.Search(2, 2);

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Equal(2, coupons.Part0.Count);
      }

      [Fact]
      public async Task GetCouponPageTenTakeTow()
      {


         var result = await couponController.Search(10, 2);

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Empty(coupons.Part0);
      }

      [Fact]
      public async Task GetAllFreeDeliveryType()
      {


         var result = await couponController.Search(1, 10, "", "FreeDelivery");

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Equal(3, coupons.Part0.Count);
      }
      [Fact]
      public async Task GetAllPercentageOfTotalType()
      {


         var result = await couponController.Search(1, 10, "", "PercentageOfTotal");

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Single(coupons.Part0);
      }
      [Fact]
      public async Task GetAllDiscountPriceType()
      {


         var result = await couponController.Search(1, 10, "", "DiscountPrice");

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Equal(2, coupons.Part0.Count);
      }

      [Fact]
      public async Task GetAllCoupon()
      {


         var result = await couponController.Search(1, 10);

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<MultiResult<List<Coupon>, int>>(objectResult.Value);

         Assert.Equal(6, coupons.Part0.Count);
      }

      [Fact]
      public async Task ValidateFree1MustBeExpired()
      {


         var result = await couponController.Validate("Free1");

         Assert.Equal(412, (result as ObjectResult).StatusCode);
         Assert.Equal("'Free1' has expired", ((result as ObjectResult).Value as List<Error>)[0].Value);
      }

      [Fact]
      public async Task ValidateTicket1MustBeNotFount()
      {


         var result = await couponController.Validate("Ticket1");

         Assert.Equal(412, (result as ObjectResult).StatusCode);
         Assert.Equal("'Ticket1' not found", ((result as ObjectResult).Value as List<Error>)[0].Value);
      }
      [Fact]
      public async Task Validate5PoundMustValid()
      {


         var result = await couponController.Validate("5Pound");

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupons = Assert.IsAssignableFrom<Coupon>(objectResult.Value);

         Assert.Equal("5Pound", coupons.Code);
      }

      [Fact]
      public async Task PostNewCouponMustBeValid()
      {


         var result = await couponController.Post(
             new Coupon
             {
                Code = "15Pound",
                DiscountAmount = 0,
                ExpiryDate = DateTime.Now.AddDays(30),
                MaxUseQuantity = 5,
                MinimumOrderPrice = 15,
                Type = Extras.CustomTypes.CouponType.DiscountPrice
             }
            );

         var objectResult = Assert.IsType<CreatedResult>(result);

         var coupons = Assert.IsAssignableFrom<Coupon>(objectResult.Value);

         Assert.Equal("15Pound", coupons.Code);
      }

      [Fact]
      public async Task PostDuplicateCouponCodeMustInvlid()
      {


         var result = await couponController.Post(
             new Coupon
             {
                Code = "5Pound",
                DiscountAmount = 0,
                ExpiryDate = DateTime.Now.AddDays(30),
                MaxUseQuantity = 5,
                MinimumOrderPrice = 15,
                Type = Extras.CustomTypes.CouponType.DiscountPrice
             }
            );

         Assert.Equal(412, (result as ObjectResult).StatusCode);
         Assert.Equal("Coupon Code already exists.", ((result as ObjectResult).Value as List<Error>)[0].Value);
      }
      [Fact]
      public async Task PostCouponWithotCodeMustBeInvalid()
      {

         couponController.ModelState.AddModelError("Name", "Coupon Code Required \n");

         var result = await couponController.Post(
             new Coupon
             {
                DiscountAmount = 0,
                ExpiryDate = DateTime.Now.AddDays(30),
                MaxUseQuantity = 5,
                MinimumOrderPrice = 15,
                Type = Extras.CustomTypes.CouponType.DiscountPrice
             }
            );

         var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

         var errorList = Assert.IsAssignableFrom<List<Error>>(objectResult.Value);

         Assert.Equal("Coupon Code Required \n", errorList[0].Value);
      }
      [Fact]
      public async Task PutCoupon()
      {
         var newDate = DateTime.Now.AddDays(32);
         var result = await couponController.Put(
             new Coupon
             {
                Code = "Free1",
                DiscountAmount = 2,
                ExpiryDate = newDate,
                MaxUseQuantity = 15,
                MinimumOrderPrice = 45,
                Type = Extras.CustomTypes.CouponType.FreeDelivery
             }
            );

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var coupon = Assert.IsAssignableFrom<Coupon>(objectResult.Value);

         Assert.Equal(45, coupon.MinimumOrderPrice);
         Assert.Equal(newDate, coupon.ExpiryDate);
         Assert.Equal(15, coupon.MaxUseQuantity);
         Assert.Equal(2, coupon.DiscountAmount);
      }
      [Fact]
      public async Task PutCouponTypeCantBeChanged()
      {

         var result = await couponController.Put(
             new Coupon
             {
                Code = "Free1",
                DiscountAmount = 0,
                ExpiryDate = DateTime.Now.AddDays(30),
                MaxUseQuantity = 5,
                MinimumOrderPrice = 45,
                Type = Extras.CustomTypes.CouponType.DiscountPrice
             }
            );

         Assert.Equal(412, (result as ObjectResult).StatusCode);
         Assert.Equal("Coupon Type Can't be Change.", ((result as ObjectResult).Value as List<Error>)[0].Value);
      }
      [Fact]
      public async Task PutCouponNotExists()
      {

         var result = await couponController.Put(
             new Coupon
             {
                Code = "Free121",
                DiscountAmount = 0,
                ExpiryDate = DateTime.Now.AddDays(30),
                MaxUseQuantity = 5,
                MinimumOrderPrice = 45,
                Type = Extras.CustomTypes.CouponType.DiscountPrice
             }
            );

         Assert.Equal(412, (result as ObjectResult).StatusCode);
         Assert.Equal("Coupon Not exists.", ((result as ObjectResult).Value as List<Error>)[0].Value);
      }

      [Fact]
      public async Task DeleteCoupon()
      {
         var result = await couponController.Delete("Free1");

         var objectResult = Assert.IsType<OkObjectResult>(result);

         var message = Assert.IsAssignableFrom<string>(objectResult.Value);

         Assert.Equal("Coupon 'Free1' was deleted", message);
      }

      [Fact]
      public async Task DeleteCouponNotExists()
      {

         var result = await couponController.Delete("Free121");
         var objectResult = Assert.IsType<NotFoundObjectResult>(result);

         var errors = Assert.IsAssignableFrom<List<Error>>(objectResult.Value);

         Assert.Equal("Coupon not found", errors[0].Value);
      }

      public void Dispose()
      {
         _context.Database.EnsureDeleted();
         _context.Dispose();
      }
   }
}
