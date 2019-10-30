using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Extensions.Configuration.PlaceholderCore;
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
                .AddConfigServer()
                .AddPlaceholderResolver()
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddDebug();
                    loggingBuilder.AddDynamicConsole();
                })
                .UseStartup<Startup>()
                .Build();
    }
    
    
}