using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using NSwag;
using NSwag.CodeGeneration.TypeScript;

using OSnack.API.Database.Models;
using OSnack.API.Extras.CustomTypes;

using P8B.Core.CSharp;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace OSnack.API.Extras
{
   public static class AppFunc
   {
      internal static int GetUserId(ClaimsPrincipal userClaimsPrincipal)
      {
         int.TryParse(userClaimsPrincipal.Claims.FirstOrDefault(
                        c => c.Type == "UserId")?.Value, out int userId);
         return userId;
      }

      internal static void MakeClientZipFile(OpenApiDocument document, bool zipIt = false)
      {
         try
         {

            /// Force ignoreing Identity user sensetive information
            foreach (var classObject in document.Definitions)
            {
               if (classObject.Key == "IdentityUserOfInteger")
               {
                  foreach (var prop in typeof(User).GetProperties())
                  {
                     if (prop.CustomAttributes.Any(i => i.AttributeType.FullName == "Newtonsoft.Json.JsonIgnoreAttribute"))
                     {
                        var property = classObject.Value.Properties.SingleOrDefault(i => i.Key.ToLower() == prop.Name.ToLower());
                        if (!string.IsNullOrEmpty(property.Key))
                           classObject.Value.Properties.Remove(property);
                     }
                  }

                  document.Definitions.Add(new KeyValuePair<string, NJsonSchema.JsonSchema>("UserBase", classObject.Value));
                  document.Definitions.Remove(classObject.Key);
                  break;
               }
            }

            TypeScriptClientGenerator tg = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings()
            {
               ClassName = "{controller}",
               GenerateDtoTypes = true,
               TypeScriptGeneratorSettings =
                     {
                        EnumStyle=NJsonSchema.CodeGeneration.TypeScript.TypeScriptEnumStyle.Enum,
                        TypeStyle=NJsonSchema.CodeGeneration.TypeScript.TypeScriptTypeStyle.Class,
                        GenerateConstructorInterface=false,
                        TemplateDirectory=Path.Combine(@$"{Directory.GetCurrentDirectory()}\StaticFiles\liquid")
                     }
            });

            string zipFilePath = Path.Combine(@$"{Directory.GetCurrentDirectory()}\StaticFiles\tsApiFiles\{document.Info.Title}.zip");
            string zipFolder = Path.Combine(@$"{Directory.GetCurrentDirectory()}\StaticFiles\tsApiFiles\{document.Info.Title}Hooks");
            if (document.Tags.Any(t => t.Name.Equals("IsModelOnly")))
               zipFolder = Path.Combine(@$"{Directory.GetCurrentDirectory()}\StaticFiles\tsApiFiles");
            try { File.Delete(zipFilePath); } catch { }
            try
            {
               foreach (var file in new DirectoryInfo(zipFolder).GetFiles())
               {
                  file.Delete();
               }
            }
            catch { }
            if (!Directory.Exists(zipFolder))
            {
               Directory.CreateDirectory(zipFolder);
               //Directory.CreateDirectory(@$"{zipFolder}");
            }
            if (document.Tags.Any(t => t.Name.Equals("IsModelOnly")))
            {
               string simpleModels = "";
               string extendedModels = "";
               string enums = "";
               foreach (var modelType in tg.GetAllDtoTypes().OrderBy(t => t.TypeName.Length))
               {
                  if (modelType.TypeName != "ErrorDto" && modelType.TypeName != "ProblemDetails")
                  {
                     switch (modelType.Type)
                     {

                        case NJsonSchema.CodeGeneration.CodeArtifactType.Enum:
                           enums += modelType.Code + '\n';
                           break;

                        case NJsonSchema.CodeGeneration.CodeArtifactType.Class:
                           if (modelType.BaseTypeName == null)
                              simpleModels += modelType.Code + '\n';
                           else
                              extendedModels += modelType.Code + '\n';
                           break;

                        default:
                           break;
                     }
                  }
               }

               using (FileStream fs1 = File.Create($"{zipFolder}\\apiModels.ts"))
               {

                  byte[] info2 = new UTF8Encoding(true).GetBytes($"{enums }{simpleModels}{extendedModels}");
                  // Add some information to the file.
                  fs1.Write(info2, 0, info2.Length);

               }
            }
            else
            {
               var modelsPath = "";
               if (document.Tags.Any(t => t.Name.Equals(AppConst.AccessPolicies.Secret) || t.Name.Equals(AppConst.AccessPolicies.TopSecret)))
                  modelsPath = "osnack-frontend-shared/src/";
               foreach (var hook in tg.GetAllCodeArtifacts())
               {

                  string tempScrtip = string.IsNullOrEmpty(hook.BaseTypeName) ?
                    hook.Code.Replace("import { @@Models@@ } from \"../../_core/apiModels\";", "") :
                    hook.Code;

                  if (!string.IsNullOrWhiteSpace(modelsPath))
                  {
                     tempScrtip = tempScrtip.Replace("../../", modelsPath);
                  }
                  using FileStream fs = File.Create($"{zipFolder}\\use{hook.TypeName}Hook.ts");
                  byte[] info = new UTF8Encoding(true).GetBytes(tempScrtip.Replace("@@Models@@", hook.BaseTypeName.Replace("@", "").Replace("string,", "")));
                  // Add some information to the file.
                  fs.Write(info, 0, info.Length);

               }
            }

            document.Tags.Clear();
            if (zipIt)
            {
               ZipFile.CreateFromDirectory(zipFolder, zipFilePath);
               CoreFunc.DeleteDirectory(zipFolder);
            }

         }
         catch (Exception)
         {
            throw;
         }
      }

      internal static IEnumerable<string> GetCurrentRequestPolicies(HttpRequest request) => GetCurrentRequestPolicies(request, out AppTypes _);

      internal static IEnumerable<string> GetCurrentRequestPolicies(HttpRequest request, out AppTypes appTypes)
      {
         request.Headers.TryGetValue("Origin", out StringValues Originvalue);
         if (AppConst.Settings.AppDomains.ClientApp.EqualCurrentCultureIgnoreCase(Originvalue))
         {
            appTypes = AppTypes.Client;
            return AppConst.Settings.AppDomains.ClientAppPolicies;
         }
         else if (AppConst.Settings.AppDomains.AdminApp.EqualCurrentCultureIgnoreCase(Originvalue))
         {
            appTypes = AppTypes.Admin;
            return AppConst.Settings.AppDomains.AdminAppPolicies;
         }
         else
         {
            appTypes = AppTypes.Invalid;
            return Array.Empty<string>();
         }
      }

      internal static string GetCencoredWord(int length)
      {
         string cencoredWord = "";
         for (int i = 0; i < length; i++)
         {
            cencoredWord += "*";
         }
         return cencoredWord;
      }

      internal static DisputeFilterTypes GetDisputeFilterTypes(bool hasClose, bool hasOpen)
      {

         return (hasClose, hasOpen) switch
         {
            (true, true) => DisputeFilterTypes.OpenAndClose,
            (true, false) => DisputeFilterTypes.Close,
            (false, true) => DisputeFilterTypes.Open,
            (_, _) => DisputeFilterTypes.None
         };

      }

      internal static void Log(string txt)
      {
         try
         {

            if (!string.IsNullOrWhiteSpace(txt))
               File.AppendAllText(Path.Combine(@$"{AppDomain.CurrentDomain.BaseDirectory}\StaticFiles\log.txt"), txt + Environment.NewLine);
         }
         catch { }
      }

      internal static bool IsDatabaseConnected(string connectionString)
      {
         try
         {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
               con.Open();
            return true;
            }
         }
         catch (Exception)
         {
            return false;
         }
      }

      /// <summary>
      /// Get File path relative to root of application
      /// e.g Folder\File.json
      /// </summary>
      /// <returns>file path or if not found string.Empty</returns>
      internal static string GetFilePath(string filePath)
      {
         string path = @$"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\{filePath}";
         if (!File.Exists(path))
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"{filePath}");
         if (!File.Exists(path))
            return string.Empty;
         return path;
      }
   }
}
