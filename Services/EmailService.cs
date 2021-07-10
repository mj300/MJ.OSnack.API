using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using MimeKit;

using Newtonsoft.Json;

using OSnack.API.Database;
using OSnack.API.Database.Models;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Services;

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSnack.API.Services
{
   public partial class EmailService
   {
      private byte[] _Attachment { set; get; }
      private LoggingService _LoggingService { get; }
      private EmailTemplate Template { get; set; }
      private EmailSettings _EmailSettings { get; }
      private OSnackDbContext _DbContext { get; }
      private IWebHostEnvironment _WebEnv { get; }
      private string RepeateSection { get; set; }

      public EmailService() { }
      public EmailService(EmailSettings emailSettings, LoggingService loggingService, IWebHostEnvironment webEnv, OSnackDbContext dbContext)
      {
         /// Set the class attributes with the objects received from their
         /// corresponding middle-ware
         _EmailSettings = emailSettings;
         _DbContext = dbContext;
         _WebEnv = webEnv;
         _LoggingService = loggingService;
      }

      private async Task SendEmailAsync(string name, string email)
      {
         /// Create a MineMessage object to setup an instance of email to be send
         MimeMessage message = new MimeMessage();

         /// Add the email settings for the sender of the email
         message.From.Add(new MailboxAddress(_EmailSettings.SenderName, _EmailSettings.Sender));

         /// Add the destination email to the message
         message.To.Add(new MailboxAddress(name, email));

         /// Add the subject of the email
         message.Subject = Template.Subject;

         /// Set the body of the email and type
         BodyBuilder bodyBuilder = new BodyBuilder()
         {
            TextBody = HtmlToPlainText(Template.HTML),
            HtmlBody = Template.HTML
         };

         /// if Email must have an attachment then add it to the multi part email
         if (_Attachment != null)
         {
            /// Open a memory stream for
            using (MemoryStream attSteam = new MemoryStream(_Attachment))
            {
               MimePart attachment = new MimePart("application", "PDF");
               attachment.Content = new MimeContent(attSteam, ContentEncoding.Default);
               attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
               attachment.ContentTransferEncoding = ContentEncoding.Base64;
               attachment.FileName = "Invoice";
               bodyBuilder.Attachments.Add(attachment);
            }
         }

         /// Set the message body to the value of multi part email
         message.Body = bodyBuilder.ToMessageBody();


         async Task trySendEmail()
         {
            /// Create a disposable "SmtpClient" object in order to send the email
            using (SmtpClient client = new SmtpClient())
            {
               /// call back method that validates the server certificate for security purposes
               client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
               {
                  /// if there are no errors in the certificate then validate the check
                  if (errors == SslPolicyErrors.None || !_EmailSettings.SslCheck)
                     return true;
                  else if (errors.Equals(SslPolicyErrors.RemoteCertificateNameMismatch) && certificate.Subject.Equals("CN=*.a2hosting.com, O=A2 Hosting, Inc., L=Ann Arbor, S=Michigan, C=US"))
                     return true;
                  else if (_EmailSettings.MailServer.Equals(sender))
                     return true;

                  /// which both should be denied
                  return false;
               };

               /// Try connecting to the smtp server using SSL connection
               await client.ConnectAsync(
                   _EmailSettings.MailServer,
                   _EmailSettings.MailPort,
                   SecureSocketOptions.SslOnConnect).ConfigureAwait(false);

               /// Pass the authentication information to the connected server to perform outgoing email request
               await client.AuthenticateAsync(_EmailSettings.Sender, _EmailSettings.Password).ConfigureAwait(false);

               /// use the smtp client to send the email
               await client.SendAsync(message).ConfigureAwait(false);

               /// disconnect the smpt client connection
               await client.DisconnectAsync(true).ConfigureAwait(false);
            }
         }
         int timerDelayms = 1000;
         /// local method to retry sending the message 4 times if email failed to send.
         /// delay by 1000, 2000, 4000, 16000 (ms)
         async Task sendMail()
         {
            try
            {
               await trySendEmail().ConfigureAwait(false);
            }
            catch (Exception)
            {
               System.Threading.Thread.Sleep(timerDelayms);
               if (timerDelayms > 16000)
               {
                  timerDelayms = 1000;
                  throw;
               }
               timerDelayms *= 2;
               await trySendEmail().ConfigureAwait(false);
            }
         }

         await sendMail().ConfigureAwait(false);
      }

      private async Task SetUserTemplate(EmailTemplateTypes EmailType)
      {
         Template = await _DbContext.EmailTemplates
           .FirstOrDefaultAsync(et => et.TemplateType.Equals(EmailType)).ConfigureAwait(false);

         if (Template == null)
            throw new Exception("Email Template cannot be found.");

         Template.PrepareHtml(_WebEnv.WebRootPath);
         Template.SetServerClasses();
      }

      private static string HtmlToPlainText(string html)
      {
         Regex[] _htmlReplaces = new[] {
            new Regex(@"<script\b[^<]*(?:(?!</script>)<[^<]*)*</script>", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(1)),
            new Regex(@"<style\b[^<]*(?:(?!</style>)<[^<]*)*</style>", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(1)),
            new Regex(@"<xml\b[^<]*(?:(?!</style>)<[^<]*)*</xml>", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(1)),
            new Regex(@"<[^>]*>", RegexOptions.Compiled),
            new Regex(@" +", RegexOptions.Compiled)
          };

         foreach (var r in _htmlReplaces)
         {
            html = r.Replace(html, " ");
         }
         var lines = html
             .Split(new[] { '\r', '\n' })
             .Select(_ => System.Net.WebUtility.HtmlDecode(_.Trim()))
             .Where(_ => _.Length > 0)
             .ToArray();
         return string.Join("\n", lines);
      }

      private void SetTemplateServerPropValue(EmailTemplateRequiredClass serverClass, object obj)
      {
         try
         {
            if (serverClass.Value.ToString().Replace(" ", "").Equals(obj.GetType().Name))
            {
               foreach (var prop in serverClass.ClassProperties)
               {
                  if (!prop.IsIgnored)
                  {
                     Type objType = obj.GetType();
                     var propValue = objType.GetProperty(prop.Name).GetValue(obj, null);
                     Type propType = null;
                     if (propValue != null)
                        propType = propValue.GetType();
                     if (propValue != null && (propType.Name.Equals("HashSet`1") || propType.Name.Equals("List`1")))
                     {
                        if (string.IsNullOrWhiteSpace(RepeateSection))
                        {
                           string finalSection = "";
                           string repeatSection = "";
                           var closingHolder = serverClass.ClassProperties.Where(cp => cp.Name.Equals(prop.Name)).LastOrDefault();
                           string closingTemplateName = prop.TemplateName;
                           if (closingHolder != null)
                              closingTemplateName = closingHolder.TemplateName;

                           int fromSection = Template.HTML.IndexOf(prop.TemplateName);
                           int toSection = Template.HTML.IndexOf(closingTemplateName) + closingTemplateName.Length;
                           repeatSection = Template.HTML.Substring(fromSection, toSection - fromSection);
                           foreach (var item in propValue as IEnumerable)
                           {
                              Type itemType = item.GetType();
                              var temp = Template.RequiredClasses.FirstOrDefault(sc => sc.Value.ToString().Replace(" ", "").Equals(item.GetType().Name));
                              if (temp != null)
                              {
                                 string repeatSectionCopy = repeatSection;
                                 foreach (var item2 in temp.ClassProperties)
                                 {
                                    var tempVal = "";
                                    var item2Val = itemType.GetProperty(item2.Name).GetValue(item, null);
                                    if (!string.IsNullOrWhiteSpace(item2Val.ToString()))
                                       tempVal = item2Val.ToString();
                                    repeatSectionCopy = repeatSectionCopy.Replace(item2.TemplateName, tempVal);
                                 }
                                 finalSection += repeatSectionCopy;
                              }
                           }
                           finalSection = finalSection.Replace(prop.TemplateName, "");
                           finalSection = finalSection.Replace(closingTemplateName, "");

                           Template.HTML = Template.HTML.Replace(repeatSection, finalSection);
                        }
                     }
                     else
                     {
                        var tempVal = "";
                        if (propValue != null && !string.IsNullOrWhiteSpace(propValue.ToString()))
                           tempVal = propValue.ToString();
                        Template.HTML = Template.HTML.Replace(prop.TemplateName, tempVal);
                        Template.Subject = Template.Subject.Replace(prop.TemplateName, tempVal);
                     }
                  }
               }
            }
         }
         catch { }
      }


   }
   public class EmailServicePathNames
   {
      [JsonProperty(PropertyName = "PasswordReset")]
      public string PasswordReset { get; set; }

      [JsonProperty(PropertyName = "ConfirmEmail")]
      public string ConfirmEmail { get; set; }

      [JsonProperty(PropertyName = "NewEmployee")]
      public string NewEmployee { get; set; }

      [JsonProperty(PropertyName = "Communication")]
      public string Communication { get; set; }

      [JsonProperty(PropertyName = "Dispute")]
      public string Dispute { get; set; }
   }
}
