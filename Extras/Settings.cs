
using Newtonsoft.Json;

using OSnack.API.Extras.Paypal;
using OSnack.API.Services;

using P8B.Core.CSharp.Models;

namespace OSnack.API.Extras
{
   /// <summary>
   /// class used to have the app setting information in memory
   /// </summary>
   internal class Settings
   {
      /// <summary>
      /// The array of allowed CORs (Cross-Origin Request) URL
      /// which are allowed to connect to the web API
      /// </summary>
      [JsonProperty(PropertyName = "OpenCors")]
      internal string[] OpenCors { get; set; }

      /// <summary>
      /// Exclude specific routes from CORs check.
      /// Must provide the URI <br/>
      /// e.g. "/Images/test.png" specific file or "/Images" Folder or "/user/post" Route
      /// </summary>
      [JsonProperty(PropertyName = "ExcludedRoutesFromCORS")]
      internal string[] ExcludedRoutesFromCORS { get; set; }

      /// <summary>
      /// Brand Name
      /// </summary>
      [JsonProperty(PropertyName = "BrandName")]
      internal string BrandName { get; set; }

      [JsonProperty(PropertyName = "MaintenanceModeStatus")]
      internal bool MaintenanceModeStatus { get; set; }

      /// <summary>
      /// App Domains
      /// </summary>
      [JsonProperty(PropertyName = "AppDomains")]
      internal AppDomains AppDomains { get; set; }

      /// <summary>
      /// Email settings 
      /// </summary>
      [JsonProperty(PropertyName = "EmailSettings")]
      internal EmailSettings EmailSettings { get; set; }

      /// <summary>
      /// Path names required for email templates
      /// </summary>
      [JsonProperty(PropertyName = "EmailServicePathNames")]
      internal EmailServicePathNames EmailServicePathNames { get; set; }

      /// <summary>
      /// PayPal Settings
      /// </summary>
      [JsonProperty(PropertyName = "PayPal")]
      internal PayPalSettings PayPal { get; set; }

      [JsonProperty(PropertyName = "ExternalLoginSecrets")]
      internal ExternalEmailSecret[] ExternalLoginSecrets { get; set; }


      [JsonProperty(PropertyName = "DbConnectionString")]
      internal string DbConnectionString { get; set; }

      [JsonProperty(PropertyName = "GooglereCAPTCHASecret")]
      internal string GooglereCAPTCHASecret { get; set; }

   }

   internal class AppDomains
   {
      /// <summary>
      /// Use for setting the domain of antiforgery token cookie
      /// </summary>
      [JsonProperty(PropertyName = "AntiforgeryCookieDomain")]
      internal string AntiforgeryCookieDomain { get; set; }

      [JsonProperty(PropertyName = "ClientApp")]
      internal string ClientApp { get; set; }
      [JsonProperty(PropertyName = "ClientAppPolicies")]
      internal string[] ClientAppPolicies { get; set; }

      [JsonProperty(PropertyName = "AdminApp")]
      internal string AdminApp { get; set; }
      [JsonProperty(PropertyName = "AdminAppPolicies")]
      internal string[] AdminAppPolicies { get; set; }
   }
}
