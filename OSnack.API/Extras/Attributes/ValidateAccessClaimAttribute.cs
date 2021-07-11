using System;
using System.ComponentModel.DataAnnotations;

namespace OSnack.API.Extras.Attributes
{
   [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
   /// <summary>
   /// Custom validation attribute to check the Access claim property for Roles
   /// </summary>
   public class ValidateAccessClaimAttribute : ValidationAttribute
   {
      /// <summary>
      /// this method will be executed when the TryValidateModel(model instance) is called
      /// this method will check if the value of the property is a valid access claim value
      /// <bold>Returns True if valid else returns false</bold>
      /// </summary>
      /// <param name="value">The value object to be checked</param>
      public override bool IsValid(object value)
      {
         switch ((string)value)
         {
            case AppConst.AccessClaims.Admin:
            case AppConst.AccessClaims.Manager:
            case AppConst.AccessClaims.Customer:
               return true;
         }
         ErrorMessage = "Invalid Access Claim";
         return false;
      }
   }
}
