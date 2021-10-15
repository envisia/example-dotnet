using System;
using Envisia.React.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ExampleProject.Web.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;

namespace ExampleProject.Web
{
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddHealthChecks();

            services
                .AddDbContext<ApplicationDbContext>(options => options
                    .UseNpgsql(
                        ConnectionString ?? Configuration.GetConnectionString("DefaultConnection"),
                        x => x.MigrationsHistoryTable("dotnet_migrations"))
                    .UseSnakeCaseNamingConvention());

            if (CurrentEnvironment.IsProduction())
            {
                services
                    .AddDataProtection()
                    .PersistKeysToDbContext<ApplicationDbContext>();
            }

            services
                .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();
            services
                .AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddTransient<UserBootstrapService>();

            ConfigureReactServices(services);
        }

        private static string ConnectionString
        {
            get
            {
                var dbUser = EnvisiaVariables.Get("DB_USER");
                var dbPassword = EnvisiaVariables.Get("DB_PASSWORD");
                if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword))
                {
                    return null;
                }

                var dbHost = EnvisiaVariables.GetOrDefault("DB_HOST", "localhost");
                var dbName = EnvisiaVariables.GetOrDefault("DB_NAME", "exampleproject");

                return $"Server={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
            }
        }
    }
}