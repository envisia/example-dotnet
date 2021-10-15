using Envisia.React.Extensions;
using Envisia.React.Extensions.StaticFiles;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.Node;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExampleProject.Web
{
    public partial class Startup
    {
        private static readonly JsonSerializerSettings JsonCamelCaseSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        private void ConfigureReactServices(IServiceCollection services)
        {
            services
                .AddJsEngineSwitcher(options => options.DefaultEngineName = NodeJsEngine.EngineName)
                .AddNode();

            services.AddReactCore(config =>
            {
                config
                    .SetLoadBabel(false)
                    .SetLoadReact(false)
                    .AddScriptWithoutTransform("~/../ClientApp/build/ssr/runtime.js")
                    .AddScriptWithoutTransform("~/../ClientApp/build/ssr/vendor.js")
                    .AddScriptWithoutTransform("~/../ClientApp/build/ssr/components.js")
                    .SetJsonSerializerSettings(JsonCamelCaseSerializerSettings)
                    .SetReuseJavaScriptEngines(!CurrentEnvironment.IsDevelopment());
            });
            
            // we use our spa static file handler, which will read the manifest.json file in production
            services.AddEvSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-Token");
        }
    }
}