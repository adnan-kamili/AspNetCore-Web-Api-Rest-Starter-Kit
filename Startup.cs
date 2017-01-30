using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.Entity.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using SampleApi.Models;
using SampleApi.Repository;
using SampleApi.Options;
using SampleApi.Filters;
using SampleApi.Policies;
using System.Reflection;

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
            // Add framework services
            services.AddAuthentication();
            services.AddOptions();
            services.Configure<AppOptions>(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySQL(Configuration.Get<AppOptions>().ConnectionStrings.MySqlProvider);
                options.UseOpenIddict();
            });
            services.AddScoped(typeof(IRepository), typeof(EFRepository<ApplicationDbContext>));

            services.AddAuthorization(options =>
            {
                Type type = typeof(Permissions);
                foreach (var permission in type.GetFields())
                {
                    var permissionValue = permission.GetValue(null).ToString();
                    options.AddPolicy(permissionValue, policy => policy.Requirements.Add(new PermissionRequirement(permissionValue)));
                }
            });
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            services.AddMvcCore(
                options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                                    .RequireAuthenticatedUser()
                                    .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                    options.Filters.Add(typeof(CustomExceptionFilter));
                    options.Filters.Add(new ValidateModelFilter());
                }
               )
               .AddJsonFormatters()
               .AddAuthorization()
               .AddDataAnnotations()
               .AddCors();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOpenIddict()
                .AddEntityFrameworkCoreStores<ApplicationDbContext>()
                .AddMvcBinders()
                .EnableTokenEndpoint("/connect/token")
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .UseJsonWebTokens()
                // You can disable the HTTPS requirement during development.
                .DisableHttpsRequirement()
                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // To be used during development
                .AddEphemeralSigningKey();


            // Add application services.
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
            var secretKey = "mysupersecret_secretkey!123";
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
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
                    IssuerSigningKey = signingKey,

                    ValidateIssuer = true,
                    //ValidIssuer = Configuration.Get<AppOptions>().Jwt.Authority,

                    ValidateAudience = true,
                    ValidAudience = Configuration.Get<AppOptions>().Jwt.Audience,

                    ValidateLifetime = true,
                }
            });

            app.UseOpenIddict();
            app.UseMvc();
            DbInitializer.Initialize(repository);
        }
    }
}
