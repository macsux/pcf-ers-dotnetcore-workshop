using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.TaskCore;

namespace Articulate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).RunWithTasks();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseCloudFoundryHosting()
                .AddCloudFoundry()
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddDynamicConsole();
                })
                .UseStartup<Startup>()
                .Build();
    }
    
    
}