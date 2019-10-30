using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Articulate.Models;
using Articulate.Repositories;
using Articulate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.EFCore;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Common.Tasks;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace Articulate.Controllers
{
    public class WithEnv : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = (Controller) filterContext.Controller;
            controller.ViewBag.AppEnv = filterContext.HttpContext.RequestServices.GetService<AppEnv>();
            controller.ViewBag.HasEurekaBinding = filterContext.HttpContext.RequestServices.GetService<IConfiguration>().GetServiceInfos<EurekaServiceInfo>().Any();
            controller.ViewBag.CFApp = filterContext.HttpContext.RequestServices.GetService<IOptionsSnapshot<CloudFoundryApplicationOptions>>().Value;
        }
    }
    [WithEnv]
    public class HomeController : Controller
    {
        private readonly IAttendeeClient _attendeeClient;
        private readonly ILogger<HomeController> _log;
        private IOptionsSnapshot<CloudFoundryApplicationOptions> _app;
        private readonly Lazy<RepositoryProvider> _provider;
        private readonly IConfiguration _config;

        public HomeController(
            IAttendeeClient attendeeClient, 
            ILogger<HomeController> log, 
            IOptionsSnapshot<CloudFoundryApplicationOptions> app, 
            Lazy<RepositoryProvider> provider,
            IConfiguration config)
        {
            _attendeeClient = attendeeClient;
            _log = log;
            _app = app;
            _provider = provider;
            _config = config;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [Route("/services")]
        public async Task<IActionResult> Attendees()
        {
            ViewBag.RepositoryProvider = _provider;
            ViewBag.IsMigrated = _attendeeClient.IsMigrated;
            ViewBag.Endpoint = _attendeeClient.Endpoint;
            ViewBag.CanConnect = _attendeeClient.CanConnect;
            return View("Attendees", _attendeeClient.CanConnect && _attendeeClient.IsMigrated ? await _attendeeClient.GetAll() : null);
            
        }
        
        [Route("/clean")]
        public async Task<IActionResult> Clean()
        {
            await _attendeeClient.DeleteAll();
            return await Attendees();
        }

        [HttpPost]
        [Route("/add-attendee")]
        public async Task<IActionResult> AddAttendee(string firstName, string lastName, string emailAddress)
        {
            await _attendeeClient.Add(firstName, lastName, emailAddress);
            return await Attendees();
        }

        [Route("/basics")]
        public IActionResult Kill(bool doIt)
        {
            if (doIt)
            {
                ViewBag.Killed = true;
                
                _log.LogWarning("*** The system is shutting down. ***");
                Task.Run(async () =>
                {
                    var name = Thread.CurrentThread.Name;
                    _log.LogWarning($"killing shortly {name}");
                    await Task.Delay(5000);
                    _log.LogWarning($"killed {name}");
                    Environment.Exit(0);
                });
            }

            return View("Basics");
        }

        [Route("/config")]
        public IActionResult Config([FromServices]IOptionsSnapshot<ColorSettings> colorConfig, bool full = false)
        {
            var placeholderProvider = ((IConfigurationRoot)_config).Providers.First() as PlaceholderResolverProvider;
            
            var dataProperty = typeof(ConfigurationProvider).GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic);
            var configByProvider = ((IConfigurationRoot)placeholderProvider.Configuration).Providers
                .OfType<ConfigurationProvider>()
                .SelectMany((provider,i) => ((IDictionary<string, string>) dataProperty.GetValue(provider))
                    .Select(x => new ProviderConfigValue
                    {
                        Provider = provider.GetType(),
                        File = provider is FileConfigurationProvider ? Path.GetFileName(((FileConfigurationProvider) provider).Source.Path) : null,
                        Index = i,
                        Key = x.Key,
                        Value = x.Value
                    }))
                .Where(x => x.Key.StartsWith("colors:", StringComparison.InvariantCultureIgnoreCase) || full)
                .ToList();

            return View((colorConfig.Value, configByProvider, full));
        }

        [Route("/ssh-file")]
        public IActionResult WriteFile()
        {
            var fileName = "ers-ssh-demo.log";
            
            System.IO.File.WriteAllText(fileName,DateTime.Now.ToString("MM-dd-yy HH:mm:ss"));
            ViewBag.SSHFile = new FileInfo(fileName).FullName;
            return View("Basics");
        }

        [Route("/migrate")]
        public async Task<IActionResult> Migrate([FromServices]IEnumerable<IApplicationTask> tasks)
        {
            var migrationTask = tasks.OfType<MigrateDbContextTask<AttendeeContext>>().FirstOrDefault();
            migrationTask?.Run();
            return await Attendees();
        }

        [Route("/bluegreen")]
        public IActionResult BlueGreen()
        {
            foreach (var envVar in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
            {
                _log.LogInformation($"{envVar.Key}: {envVar.Value}");
            }

            return View();
        }

        [Route("/bluegreen-check")]
        public string[] BlueGreenCheck([FromServices]IOptionsSnapshot<CloudFoundryApplicationOptions> options)
        {
            return new []
            {
                options.Value.Application_Name, 
                options.Value.InstanceIndex.ToString()
            };
        }
        
    }
}