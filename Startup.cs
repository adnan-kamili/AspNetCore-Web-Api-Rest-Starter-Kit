using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleApi.Repository;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.Entity.Extensions;
using Microsoft.AspNetCore.Identity;

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
            // Add framework services.
            // services.AddIdentity<ApplicationUser, IdentityRole>()
            //     .AddEntityFrameworkStores<ApplicationDbContext>()
            //     .AddDefaultTokenProviders();
            var sqlConnectionString = "server=localhost;userid=adnan;password=adnan;database=sample-db;";
            services.AddDbContext<ApplicationDbContext>(options => options.UseMySQL(sqlConnectionString));
            services.AddScoped(typeof(IRepository), typeof(EFRepository<ApplicationDbContext>));
            // services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase());
            services.AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization()
                .AddDataAnnotations()
                .AddCors();


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
            app.UseMvc();
            DbInitializer.Initialize(repository);
            Console.WriteLine("configure middleware");
        }
    }
}
