using OSnack.API.Database.Models;
using OSnack.API.Extras;
using OSnack.API.Extras.CustomTypes;

using System;
using System.Threading.Tasks;

namespace OSnack.API.Services
{
   public partial class EmailService
   {
      public async Task<bool> EmailConfirmationAsync(User user, string DomainUrl)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.EmailConfirmation).ConfigureAwait(false);

            Token token = new Token()
            {
               Type = TokenTypes.ConfirmEmail,
               UrlDomain = DomainUrl,
            };
            token.GenerateToken(user, DateTime.UtcNow.AddYears(1), _DbContext, AppConst.Settings.EmailServicePathNames.ConfirmEmail);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, user);
               SetTemplateServerPropValue(serverClass, token);
            }

            await SendEmailAsync($"{user.FirstName} {user.Surname}", user.Email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> WelcomeExternalRegistrationAsync(User user)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.WelcomeExternalRegistration).ConfigureAwait(false);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, user);
               SetTemplateServerPropValue(serverClass, user.RegistrationMethod);
            }

            await SendEmailAsync($"{user.FirstName} {user.Surname}", user.Email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> WelcomeNewEmployeeAsync(User user, string DomainUrl)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.WelcomeNewEmployee).ConfigureAwait(false);

            Token token = new Token()
            {
               Type = TokenTypes.ChangePassword,
               UrlDomain = DomainUrl,
            };
            token.GenerateToken(user, DateTime.UtcNow.AddDays(2), _DbContext, AppConst.Settings.EmailServicePathNames.NewEmployee);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, user);
               SetTemplateServerPropValue(serverClass, user.Role);
               SetTemplateServerPropValue(serverClass, token);
            }

            await SendEmailAsync($"{user.FirstName} {user.Surname}", user.Email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> PasswordResetAsync(User user, string DomainUrl)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.PasswordReset).ConfigureAwait(false);
            Token token = new Token()
            {
               Type = TokenTypes.ChangePassword,
               UrlDomain = DomainUrl,
            };
            token.GenerateToken(user, DateTime.UtcNow.AddHours(5), _DbContext, AppConst.Settings.EmailServicePathNames.PasswordReset);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, user);
               SetTemplateServerPropValue(serverClass, token);
            }

            await SendEmailAsync($"{user.FirstName} {user.Surname}", user.Email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> OrderReceiptAsync(Order order)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.OrderReceipt).ConfigureAwait(false);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, order);
               SetTemplateServerPropValue(serverClass, order.Payment);
            }

            string email = order.Payment.Email;

            if (order.User != null)
               email = order.User.Email;
            await SendEmailAsync($"{order.Name}", email).ConfigureAwait(false);

            await SetUserTemplate(EmailTemplateTypes.OrderReceiptForAdmin).ConfigureAwait(false);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, order);
               SetTemplateServerPropValue(serverClass, order.Payment);
            }


            await SendEmailAsync(AppConst.Settings.EmailSettings.AdminEmail, AppConst.Settings.EmailSettings.AdminEmail).ConfigureAwait(false);

            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> OrderDisputeAsync(Order order, Communication communication)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.OrderDispute).ConfigureAwait(false);

            communication.SetURL($"{AppConst.Settings.AppDomains.ClientApp}{AppConst.Settings.EmailServicePathNames.Dispute}");
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, order);
               SetTemplateServerPropValue(serverClass, communication);
            }

            string email = order.Payment.Email;

            if (order.User != null)
               email = order.User.Email;
            await SendEmailAsync($"{order.Name}", email).ConfigureAwait(false);

            await SetUserTemplate(EmailTemplateTypes.OrderDisputeForAdmin).ConfigureAwait(false);


            communication.SetURL($"{AppConst.Settings.AppDomains.AdminApp}{AppConst.Settings.EmailServicePathNames.Dispute}");
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, order);
               SetTemplateServerPropValue(serverClass, communication);
            }

            await SendEmailAsync(AppConst.Settings.EmailSettings.AdminEmail, AppConst.Settings.EmailSettings.AdminEmail).ConfigureAwait(false);

            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> OrderCancelationAsync(Order order, Communication dispute)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.OrderReceipt).ConfigureAwait(false);
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, order);
               SetTemplateServerPropValue(serverClass, order.Payment);
            }

            string email = order.Payment.Email;

            if (order.User != null)
               email = order.User.Email;
            await SendEmailAsync($"{order.Name}", email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> MessageToAdminAsync(Message message, Communication communication)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.MessageToAdmin).ConfigureAwait(false);
            if (communication.Type == ContactType.Dispute)
               communication.SetURL($"{AppConst.Settings.AppDomains.AdminApp}{AppConst.Settings.EmailServicePathNames.Dispute}");
            if (communication.Type == ContactType.Message)
               communication.SetURL($"{AppConst.Settings.AppDomains.AdminApp}{AppConst.Settings.EmailServicePathNames.Communication}");
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, message);
               SetTemplateServerPropValue(serverClass, communication);
            }

            await SendEmailAsync(AppConst.Settings.EmailSettings.AdminEmail, AppConst.Settings.EmailSettings.AdminEmail).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }

      public async Task<bool> MessageToUser(Message message, Communication communication)
      {
         try
         {
            await SetUserTemplate(EmailTemplateTypes.MessageToUser).ConfigureAwait(false);
            if (communication.Type == ContactType.Dispute)
               communication.SetURL($"{AppConst.Settings.AppDomains.AdminApp}{AppConst.Settings.EmailServicePathNames.Dispute}");
            if (communication.Type == ContactType.Message)
               communication.SetURL($"{AppConst.Settings.AppDomains.AdminApp}{AppConst.Settings.EmailServicePathNames.Communication}");
            foreach (EmailTemplateRequiredClass serverClass in Template.RequiredClasses)
            {
               SetTemplateServerPropValue(serverClass, message);
               SetTemplateServerPropValue(serverClass, communication);
            }

            await SendEmailAsync(communication.FullName, communication.Email).ConfigureAwait(false);
            return true;
         }
         catch (Exception ex)
         {
            _LoggingService.LogEmailFailure(ex.Message, new { ex });
            return false;
         }
      }
   }
}
