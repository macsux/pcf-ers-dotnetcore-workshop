using System;
using System.Collections.Generic;
using System.Linq;
using Articulate.Models;
using Articulate.Repositories;
using Articulate.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.EFCore;
using Steeltoe.Common.Tasks;
using Steeltoe.Discovery.Client;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.DbMigrations;
using Steeltoe.Management.Endpoint.Env;
using Steeltoe.Management.Endpoint.Refresh;
using Steeltoe.Management.Hypermedia;
using Steeltoe.Management.TaskCore;
using Steeltoe.Management.Tracing;

namespace Articulate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.ConfigureCloudFoundryOptions(Configuration); // register options that parse VCAP_APPLICATION & VCAP_SERVICES
            services.AddSingleton(Configuration);
            
            services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry); // standard actuators integrated in apps manager
            services.AddDbMigrationsActuator(Configuration); // actuator that shows which EF migrations have been applied
            services.AddEnvActuator(Configuration); // actuator that shows info about environment
            services.AddRefreshActuator(Configuration); // actuator to allow refreshing config
            services.AddDistributedTracing(Configuration); // propagates distributed tracing http headers to downstream calls
            services.AddTask<MigrateDbContextTask<AttendeeContext>>(ServiceLifetime.Transient); // registers task that migrates database. invoked by RunWithTasks in Programs.cs
            
            services.AddScoped<AppEnv>();
            services.AddSingleton<AppState>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<ColorSettings>(Configuration.GetSection("colors"));
            services.AddAtteendeeClient(Configuration); // register different backend implementation based on config
            services.AddDiscoveryClient(Configuration);
            services.AddHttpClient<ApiAttendeeClient>(http => http.BaseAddress = new Uri(Configuration.GetValue<string>("backendUrl")));
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAttendeeClient c)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

//            logger.LogInformation(string.Join("\n", Configuration.AsEnumerable().Select(item => $"{item.Key}: {item.Value}")));
            
            app.UseFileServer(); // serve static content from wwwroot
            
            // enable actuators middleware
            app.UseCloudFoundryActuators(MediaTypeVersion.V2,ActuatorContext.ActuatorAndCloudFoundry); 
            app.UseEnvActuator();
            app.UseRefreshActuator();
            app.UseDbMigrationsActuator();
            app.UseDiscoveryClient();
            app.UseAtteendeeClient();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            

        }
    }
}