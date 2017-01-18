using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.Entity.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SampleApi.Models;
using SampleApi.Repository;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services

            var sqlConnectionString = "server=localhost;userid=adnan;password=adnan;database=sample-db;";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySQL(sqlConnectionString);
                // Register the entity sets needed by OpenIddict.
                options.UseOpenIddict();
            });
            services.AddScoped(typeof(IRepository), typeof(EFRepository<ApplicationDbContext>));
            // services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase());

            services.AddMvcCore(config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                                    .RequireAuthenticatedUser()
                                    .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonFormatters()
                .AddAuthorization()
                .AddDataAnnotations()
                .AddCors();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOpenIddict()
    // Register the Entity Framework stores.
    .AddEntityFrameworkCoreStores<ApplicationDbContext>()

    // Register the ASP.NET Core MVC binder used by OpenIddict.
    // Note: if you don't call this method, you won't be able to
    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
    .AddMvcBinders()

    // Enable the token endpoint (required to use the password flow).
    .EnableTokenEndpoint("/connect/token")

    // Allow client applications to use the grant_type=password flow.
    .AllowPasswordFlow()

     .AllowRefreshTokenFlow()

    // During development, you can disable the HTTPS requirement.
    .DisableHttpsRequirement()

    .UseJsonWebTokens()

    // Register a new ephemeral key, that is discarded when the application
    // shuts down. Tokens signed using this key are automatically invalidated.
    // This method should only be used during development.
    .AddEphemeralSigningKey();

            services.AddAuthentication();
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
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                //ValidateIssuer = true,
                //ValidIssuer = "ExampleIssuer",

                // Validate the JWT Audience (aud) claim
                ValidateAudience = false,
                ValidAudience = "http://localhost:5000/",

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                //ClockSkew = TimeSpan.Zero
            };
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                TokenValidationParameters = tokenValidationParameters,
                Audience = "http://localhost:5000/",
                Authority = "http://localhost:5000/"
            });

            app.UseOpenIddict();
            app.UseMvc();
            DbInitializer.Initialize(repository);
        }
    }
}
