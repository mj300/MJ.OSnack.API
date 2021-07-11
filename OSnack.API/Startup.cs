using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using OSnack.API.Database;
using OSnack.API.Database.Context.ClassOverrides;
using OSnack.API.Database.Models;
using OSnack.API.Extras;

using P8B.Core.CSharp.Models;
using P8B.UK.API.Extras.Overrides;

using System;
using System.IO;
using System.Linq;

namespace OSnack.API
{
   public class Startup
   {
      internal IConfiguration Configuration { get; }
      internal const string AuthSchemeApplication = "Identity.Application";
      internal const string CoresPolicy = "CorsPolicy";
      public Startup(IConfiguration configuration) => Configuration = configuration;

      public void ConfigureServices(IServiceCollection services)
      {
         if (Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value == "Development")
         {
            AppConst.CallerDomain = "localhost";
         }
         else
         {
            var contentRootArr = Configuration.GetSection("contentRoot").Value.Split(@"\").ToList();
            AppConst.CallerDomain = contentRootArr.Last().Replace(".", "-");
         }
         AppFunc.Log(JsonConvert.SerializeObject(AppConst.Settings.OpenCors));
         /// Enable API calls from specified origins only
         services.AddCors(options =>
         {
            options.AddPolicy(CoresPolicy,
                builder => builder
                .AllowCredentials()
                .WithOrigins(AppConst.Settings.OpenCors)
                .AllowAnyMethod()
                .WithHeaders(
                "Accept",
                "Origin",
                "Content-Type",
                "X-AF-TOKEN"
                ));
         });




         /// Add the anti-forgery service and identify the
         /// the header name of the request to identify and validate the token
         services.AddAntiforgery(options =>
         {
            options.HeaderName = "X-AF-TOKEN";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
         });


         /// Pass the SQL server connection to the db context
         /// receive the connection string from the settings.json
         services.AddDbContext<OSnackDbContext>(options => options
           .UseSqlServer(AppConst.Settings.DbConnectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

         /// Add .Net Core Identity to the pipe-line with the following options
         services.AddIdentityCore<User>(options =>
         {
            options.ClaimsIdentity.UserIdClaimType = "UserId";
            options.ClaimsIdentity.SecurityStampClaimType = "SecurityStamp";
            options.ClaimsIdentity.RoleClaimType = AppConst.AccessClaims.Type;
            options.User.RequireUniqueEmail = true;
            options.Password = new PasswordOptions
            {
               RequireDigit = true,
               RequiredLength = 8,
               RequiredUniqueChars = 1,
               RequireLowercase = true,
               RequireNonAlphanumeric = true,
               RequireUppercase = true
            };
         })
         .AddEntityFrameworkStores<OSnackDbContext>()// Add the custom db context class
         .AddSignInManager<OSnackSignInManager<User>>() // add the custom SignInManager class
         .AddDefaultTokenProviders(); // Allow the use of tokens

         services.Replace(ServiceDescriptor.Scoped<IUserValidator<User>,
           OSnackUserValidator<User>>());

         /// local static function to set the cookie authentication option
         static void CookieAuthOptions(CookieAuthenticationOptions options)
         {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/AccessDenied";
            options.ClaimsIssuer = "OSnack";
            options.ExpireTimeSpan = TimeSpan.FromDays(2);
            options.SlidingExpiration = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
         }
         /// Add authentication services to the pipeline
         services.AddAuthentication().AddCookie(AuthSchemeApplication, CookieAuthOptions);

         /// Add Authorization policies for Admin, Manager, Staff and Customer
         services.AddAuthorization(options =>
         {
            options.AddPolicy(AppConst.AccessPolicies.Public, policy =>
            {
               policy.AuthenticationSchemes.Add(AuthSchemeApplication);
               policy.Requirements.Add(new EmptyRequirement());
            });
            options.AddPolicy(AppConst.AccessPolicies.Official, policy =>
            {
               policy.AuthenticationSchemes.Add(AuthSchemeApplication);
               policy.RequireAuthenticatedUser();
               policy.RequireClaim(AppConst.AccessClaims.Type
                                   , new string[] { AppConst.AccessClaims.Admin,
                                                    AppConst.AccessClaims.Manager,
                                                    AppConst.AccessClaims.Customer});
            });
            options.AddPolicy(AppConst.AccessPolicies.Secret, policy =>
            {
               policy.AuthenticationSchemes.Add(AuthSchemeApplication);
               policy.RequireAuthenticatedUser();
               policy.RequireClaim(AppConst.AccessClaims.Type
                                   , new string[] { AppConst.AccessClaims.Admin,
                                                    AppConst.AccessClaims.Manager });
            });
            options.AddPolicy(AppConst.AccessPolicies.TopSecret, policy =>
            {
               policy.AuthenticationSchemes.Add(AuthSchemeApplication);
               policy.RequireAuthenticatedUser();
               policy.RequireClaim(AppConst.AccessClaims.Type
                                   , new string[] { AppConst.AccessClaims.Admin });
            });
         });
         services.AddSingleton<IAuthorizationHandler, EmptyHandler>();

         services.Configure<ApiBehaviorOptions>(options =>
         options.SuppressModelStateInvalidFilter = true);

         //// Add MVC services to the pipeline
         services.AddMvc(options => options.EnableEndpointRouting = false)
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .WithRazorPagesRoot("/Extras/RootPage");


         services.AddControllers().AddNewtonsoftJson();

         services.AddOpenApiDocument(document =>
         {
            document.DocumentName = $"OSnack Models";
            document.Title = $"Models";
            document.AuthorizationPolicyNames = Array.Empty<string>();
            document.IsModelOnly = true;
         });
         // Register the Swagger services
         foreach (var policy in AppConst.AccessPolicies.List)
         {
            services.AddOpenApiDocument(document =>
            {
               document.DocumentName = $"OSnack {policy}";
               document.Title = $"{policy}";
               document.AuthorizationPolicyNames = new string[] { policy };
               document.IsModelOnly = false;
            });
         }
      }
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         app.UseHttpsRedirection();
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles(new StaticFileOptions
            {
               FileProvider = new PhysicalFileProvider(
                  Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles"))
            });
            //// Register the Swagger generator and the Swagger UI middle-wares
            app.UseOpenApi(config =>
            {
               config.PostProcess = (document, request) =>
                             AppFunc.MakeClientZipFile(document);
               config.Path = "/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUi3(config =>
            {
               config.ValidateSpecification = true;
            });
         }
         else
         {
            app.UseHsts();
         }
         /// Allow the use of static files from wwwroot folder
         app.UseStaticFiles(new StaticFileOptions()
         {
            OnPrepareResponse = ctx =>
            {
               ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
               ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
                 "Origin, X-Requested-With, Content-Type, Accept");
            },
         });

         app.UseCors(CoresPolicy);
         app.Use(next => context =>
         {
            if (string.IsNullOrWhiteSpace(AppConst.Settings.DbConnectionString))
            {
               context.Request.Path = "/authentication/get/databaseconnectionfailed";
               return next(context);
            }
            foreach (string element in AppConst.Settings.ExcludedRoutesFromCORS)
            {
               if (context.Request.Path.StartsWithSegments(new PathString(element)))
                  return next(context);
            }
            string OrgPath = context.Request.Path;
            context.Request.Path = "/";
            string logReport = $"Path => {OrgPath}. {Environment.NewLine} No Match Found ({context.Request.Host})";
            if (context.Request.Headers.TryGetValue("Origin", out StringValues OriginValue))
            {
               logReport = "";
               logReport += $"Path => {OrgPath} {Environment.NewLine}";
               logReport += $"Method => {context.Request.Method} {Environment.NewLine}";
               foreach (var COR in AppConst.Settings.OpenCors)
               {
                  if (COR.EqualCurrentCultureIgnoreCase(OriginValue))
                  {
                     logReport = "";
                     logReport += $"Method => {OrgPath} {Environment.NewLine}";
                     logReport += $"Method => {context.Request.Method} {Environment.NewLine}";
                     context.Request.Path = OrgPath;
                     logReport += $"Cor Success => {COR} {Environment.NewLine}";
                     break;
                  }
                  else
                  {
                     logReport += $"Cor failed => {COR} is not {OriginValue}{Environment.NewLine}";
                  }
               }
            }
            AppFunc.Log(logReport + Environment.NewLine);
            return next(context);
         });
         /// Enable the application to use authentication
         app.UseAuthentication();

         app.UseMvc();
      }
   }
}
