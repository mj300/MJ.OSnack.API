using Newtonsoft.Json;

using System;
using System.IO;
using System.Reflection;

namespace OSnack.API.Extras
{
   internal static class AppConst
   {

      /// <summary>
      /// Three Levels of access claims within the system.<br />
      /// * Admin<br/>
      /// * Manager<br/>
      /// * Customer
      /// </summary>
      internal struct AccessClaims
      {
         internal const string Type = "Role";
         internal const string Admin = nameof(Admin);
         internal const string Manager = nameof(Manager);
         internal const string Customer = nameof(Customer);
         internal static readonly string[] List =
         { Type, Admin, Manager, Customer  };
      }

      /// <summary>
      /// Four Levels of access policies within the system.<br />
      /// </summary>
      internal struct AccessPolicies
      {
         /// <summary>
         /// TopSecret Policy includes the following Roles <br/>
         /// * Admin <br/>
         /// </summary>
         internal const string TopSecret = nameof(TopSecret);

         /// <summary>
         /// Secret Policy includes the following Roles <br/>
         /// * Admin <br/>
         /// * Manager <br/>
         /// </summary>
         internal const string Secret = nameof(Secret);

         /// <summary>
         /// Official Policy includes the following Roles <br/>
         /// * Admin <br/>
         /// * Manager <br/>
         /// * Customer <br/>
         /// </summary>
         internal const string Official = nameof(Official);
         /// <summary>
         /// Public Policy includes Everyone including anonymous 
         /// </summary>
         internal const string Public = nameof(Public);

         internal static string[] List
         {
            get => new string[] {
            nameof(TopSecret),
            nameof(Secret),
            nameof(Official),
            nameof(Public),
         };
         }
      }

      internal enum RegistrationTypes
      {
         Application = 0,
         Google = 1,
         Facebook = 2,
         Github = 3
      }

      internal static string CallerDomain { get; set; }

      private static Settings _settings;
      /// <summary>
      /// Get the information from the appSettings json file
      /// </summary>
      internal static Settings Settings
      {
         get
         {
            if (_settings == null || string.IsNullOrWhiteSpace(_settings.DbConnectionString))
               SetSettings();
            return _settings;
         }
      }

      internal static void SetSettings()
      {
         _settings = new Settings();
         string settingsPath = AppFunc.GetFilePath(@"StaticFiles\Settings.json");
         string domainSettingsPath = AppFunc.GetFilePath(@$"StaticFiles\Settings.{CallerDomain}.json");
         if (string.IsNullOrWhiteSpace(settingsPath) || string.IsNullOrWhiteSpace(domainSettingsPath))
            throw new Exception("File Not Found");

         JsonConvert.PopulateObject(File.ReadAllText(settingsPath), _settings);
         JsonConvert.PopulateObject(File.ReadAllText(domainSettingsPath), _settings);

         if (!AppFunc.IsDatabaseConnected(_settings.DbConnectionString))
            _settings.DbConnectionString = "";
      }
   }
}
