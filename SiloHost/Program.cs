using Grains;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using SiloHost.HealthCheck.Grains;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace SiloHost
{
    public class Program
    {
        public static async Task Main()
        {
            await CreateHostBuilder()
                .Build()
                .RunAsync();
        }

        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(AddAppConfiguration)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((builderContext, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                    services.Configure<AdoNetClusteringConfiguration>(
                        builderContext.Configuration.GetSection(AdoNetClusteringConfiguration.AdoNetClustering));
                })
                .UseOrleans((builderContext, siloBuilder) =>
                {
                    var adoNetClusteringConfiguration = builderContext.Configuration.GetSection(AdoNetClusteringConfiguration.AdoNetClustering)
                                                                                    .Get<AdoNetClusteringConfiguration>();
                    siloBuilder
                     .Configure<ClusterOptions>(options =>
                     {
                         options.ClusterId = adoNetClusteringConfiguration.ClusterId;
                         options.ServiceId = adoNetClusteringConfiguration.ServiceId;
                     })
                     .Configure<EndpointOptions>(options =>
                     {
                         options.AdvertisedIPAddress = builderContext.HostingEnvironment.IsStaging() ?
                            Dns.GetHostEntry(Dns.GetHostName()).AddressList.First() : IPAddress.Loopback;
                         options.SiloPort = EndpointOptions.DEFAULT_SILO_PORT;
                         options.GatewayPort = EndpointOptions.DEFAULT_GATEWAY_PORT;
                     })
                     .UseAdoNetClustering(options =>
                     {
                         options.Invariant = adoNetClusteringConfiguration.Invariant;
                         options.ConnectionString = adoNetClusteringConfiguration.ConnectionString;
                     })
                     .ConfigureApplicationParts(parts =>
                     {
                         parts.AddApplicationPart(typeof(PublisherGrain).Assembly).WithReferences();
                         parts.AddApplicationPart(typeof(LocalHealthCheckGrain).Assembly).WithReferences();
                     })
                     .ConfigureServices(services =>
                     {
                         services.AddHostedService<StreamSubscribeWorker>();
                     })
                     .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddSerilog())
                     .AddSimpleMessageStreamProvider("SMSProvider")
                     .AddMemoryGrainStorage("PubSubStore")
                     .AddMemoryGrainStorageAsDefault();
                })
                .UseConsoleLifetime()
                .UseSerilog((builderContext, config) =>
                {
                    config.ReadFrom.Configuration(builderContext.Configuration);
                });

            static void AddAppConfiguration(HostBuilderContext hostingContext, IConfigurationBuilder config)
            {
                var env = hostingContext.HostingEnvironment;
                config.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            }
        }
    }
}
