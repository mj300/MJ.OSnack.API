using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace P8B.UK.API.Extras.Overrides
{
   public class OSnackUserValidator<TUser> : IUserValidator<TUser> where TUser : class
   {
      public IdentityErrorDescriber Describer { get; }
      /// <summary>
      /// Validates the specified <paramref name="user"/> as an asynchronous operation.
      /// </summary>
      /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
      /// <param name="user">The user to validate.</param>
      /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
      public virtual async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
      {
         if (manager == null)
         {
            throw new ArgumentNullException(nameof(manager));
         }
         var errors = new List<IdentityError>();
         if (manager.Options.User.RequireUniqueEmail)
         {
            await ValidateEmail(manager, user, errors).ConfigureAwait(false);
         }
         return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
      }
      // make sure email is not empty, valid, and unique
      private async Task ValidateEmail(UserManager<TUser> manager, TUser user, List<IdentityError> errors)
      {
         var email = await manager.GetEmailAsync(user).ConfigureAwait(false);
         if (string.IsNullOrWhiteSpace(email))
         {
            errors.Add(Describer.InvalidEmail(email));
            return;
         }
         if (!new EmailAddressAttribute().IsValid(email))
         {
            errors.Add(Describer.InvalidEmail(email));
            return;
         }
         var owner = await manager.FindByEmailAsync(email).ConfigureAwait(false);
         if (owner != null &&
             !string.Equals(await manager.GetUserIdAsync(owner).ConfigureAwait(false), await manager.GetUserIdAsync(user).ConfigureAwait(false)))
         {
            errors.Add(Describer.DuplicateEmail(email));
         }
      }
   }
}
