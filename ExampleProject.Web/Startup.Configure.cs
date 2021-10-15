using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExampleProject.Web
{
    public partial class Startup
    {
        private const string ClientAppPath = "/dist";

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (env.IsProduction())
            {
                app.Map(ClientAppPath, innerApp =>
                {
                    innerApp.UseSpaStaticFiles();
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(
                    "/healthz/ready",
                    new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
                endpoints.MapHealthChecks(
                    "/healthz/live",
                    new HealthCheckOptions { Predicate = _ => false });

                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });

            if (env.IsDevelopment())
            {
                app.Map(ClientAppPath, innerApp =>
                {
                    innerApp.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = "ClientApp";
                        spa.UseReactDevelopmentServer(npmScript: "build:dev");
                    });
                });
            }
        }
    }
}