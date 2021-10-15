using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ExampleProject.Web
{
    public partial class Startup
    {        
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; }
        
        public Startup(IWebHostEnvironment currentEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentEnvironment = currentEnvironment;
        }
    }
}