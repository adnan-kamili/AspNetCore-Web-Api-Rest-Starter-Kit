using System;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MySQL.Data.Entity.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using AspNet.Security.OpenIdConnect.Primitives;

using SampleApi.Models;
using SampleApi.Repository;
using SampleApi.Options;
using SampleApi.Filters;
using SampleApi.Policies;


namespace SampleApi
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add options
            services.AddOptions();
            services.Configure<AppOptions>(Configuration);

            // Add database Context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySQL(Configuration.Get<AppOptions>().ConnectionStrings.MySqlProvider);
                options.UseOpenIddict();
            });
            services.AddScoped(typeof(IRepository), typeof(EFRepository<ApplicationDbContext>));

            // Add identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                // Create a policy for each permission
                Type type = typeof(PermissionClaims);
                foreach (var permissionClaim in type.GetFields())
                {
                    var permissionValue = permissionClaim.GetValue(null).ToString();
                    options.AddPolicy(permissionValue, policy => policy.Requirements.Add(new PermissionRequirement(permissionValue)));
                }
            });
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            // Add MVC Core
            services.AddMvcCore(
                options =>
                {
                    // Add global authorization filter 
                    var policy = new AuthorizationPolicyBuilder()
                                    .RequireAuthenticatedUser()
                                    .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));

                    // Add global exception handler for production
                    options.Filters.Add(typeof(CustomExceptionFilterAttribute));

                    // Add global validation filter
                    options.Filters.Add(typeof(ValidateModelFilterAttribute));

                    // Add global tenant filter
                    options.Filters.Add(typeof(TenantFilterAttribute));
                }
               )
               .AddJsonFormatters()
               .AddAuthorization()
               .AddDataAnnotations()
               .AddCors();



            // Add authentication
            services.AddAuthentication();

            // Configure Identity to use the same JWT claims as OpenIddict 
            services.Configure<IdentityOptions>(options =>
            {
               options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
               options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Add OpenId Connect/OAuth2
            services.AddOpenIddict()
                .AddEntityFrameworkCoreStores<ApplicationDbContext>()
                .AddMvcBinders()
                .EnableTokenEndpoint("/connect/token")
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .UseJsonWebTokens()
                // You can disable the HTTPS requirement during development or if behind a reverse proxy
                .DisableHttpsRequirement()
                .AddSigningKey(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.Get<AppOptions>().Jwt.SecretKey)));
            // Register a new ephemeral key, that is discarded when the application
            // shuts down. Tokens signed using this key are automatically invalidated.
            // To be used during development
            //.AddEphemeralSigningKey();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IRepository repository)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var secretKey = Configuration.Get<AppOptions>().Jwt.SecretKey;
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = env.IsProduction(),
                Audience = Configuration.Get<AppOptions>().Jwt.Audience,
                Authority = Configuration.Get<AppOptions>().Jwt.Authority,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),

                    ValidateIssuer = true,
                    // makes no difference seemingly being ignored
                    //ValidIssuer = Configuration.Get<AppOptions>().Jwt.Authority,

                    ValidateAudience = true,
                    ValidAudience = Configuration.Get<AppOptions>().Jwt.Audience,

                    ValidateLifetime = true,
                }
            });

            // Add OpedId Connect middleware
            app.UseOpenIddict();

            // Add Mvc middleware
            app.UseMvc();

            // Database intializer
            repository.EnsureDatabaseCreated();
            //DbInitializer.Initialize(repository);
        }
    }
}
