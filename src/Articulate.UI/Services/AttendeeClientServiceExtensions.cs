using System;
using System.Data;
using System.Linq;
using Articulate.Models;
using Articulate.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;

namespace Articulate.Services
{
    public static class AttendeeClientServiceExtensions
    {
        public static IServiceCollection AddAtteendeeClient(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = new Lazy<RepositoryProvider>(() => GetProvider(configuration));
            services.AddSingleton(provider);

            if (provider.Value != RepositoryProvider.Rest)
            {
                services.AddDbContext<AttendeeContext>(options =>
                    {

                        switch (provider.Value)
                        {
                            case RepositoryProvider.SqlServer:
                                options.UseSqlServer(configuration);
                                break;
                            case RepositoryProvider.MySql:
                                options.UseMySql(configuration);
                                break;
                            case RepositoryProvider.Memory:
                                options.UseSqlite("DataSource=:memory:");
                                break;
                        }
                    }, provider.Value != RepositoryProvider.Memory ? ServiceLifetime.Scoped : ServiceLifetime.Singleton);
                services.AddScoped<IAttendeeClient, DatabaseAttendeeClient>();
            }
            else
            {
                //services.AddSingleton<IAttendeeClient, ApiAttendeeClient>();  <-- this syntax won't work due to a bug in how HttpClientFactory registrations work
                services.AddSingleton<IAttendeeClient>(c => c.GetRequiredService<ApiAttendeeClient>());
            }

            return services;
        }

        private static IDbConnection _connection;
        public static IApplicationBuilder UseAtteendeeClient(this IApplicationBuilder app)
        {
            // if using in sqlite memory provider, we should keep at least one active connection open to the db otherwise it's gonna get scrapped along with schema
            var dbClient = app.ApplicationServices.CreateScope().ServiceProvider.GetService<AttendeeContext>();
            if (dbClient == null || dbClient.Database.ProviderName != "Sqlite")
                return app;
            _connection = dbClient.Database.GetDbConnection();
            _connection.Open();
            return app;
        }
        
        private static RepositoryProvider GetProvider(IConfiguration configuration)
        {
            var provider = configuration.GetValue<RepositoryProvider?>("repositoryProvider");
            if (provider != null)
                return provider.Value;

            var urlConfig = new UrlServiceOptions();
            urlConfig.Bind(configuration,"rest");
            if (urlConfig.Name != null)
                return RepositoryProvider.Rest;
            if (configuration.GetServiceInfos<MySqlServiceInfo>().Any())
                return RepositoryProvider.MySql;
            if (configuration.GetServiceInfos<SqlServerServiceInfo>().Any())
                return RepositoryProvider.SqlServer;
            return RepositoryProvider.Memory;
        }

    }
    public enum RepositoryProvider
    {
        Memory,
        MySql,
        SqlServer,
        Rest
    }
}