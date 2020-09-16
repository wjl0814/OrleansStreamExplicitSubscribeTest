using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

namespace API
{
    public class ClusterClientHostedService : IHostedService
    {
        private readonly ILogger<ClusterClientHostedService> _logger;
        private readonly AdoNetClusteringConfiguration _adoNetClusteringConfiguration;
        public IClusterClient Client { get; }

        public ClusterClientHostedService(ILogger<ClusterClientHostedService> logger,
            IOptions<AdoNetClusteringConfiguration> adoNetClusteringConfiguration, ILoggerProvider loggerProvider)
        {
            _logger = logger;
            _adoNetClusteringConfiguration = adoNetClusteringConfiguration.Value;

            Client = new ClientBuilder()
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = _adoNetClusteringConfiguration.ClusterId;
                   options.ServiceId = _adoNetClusteringConfiguration.ServiceId;
               })
               .UseAdoNetClustering(options =>
               {
                   options.Invariant = _adoNetClusteringConfiguration.Invariant;
                   ;
                   options.ConnectionString = _adoNetClusteringConfiguration.ConnectionString;
               })
               .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddProvider(loggerProvider))
               .Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var attempt = 0;
            var maxAttempts = 100;
            var delay = TimeSpan.FromSeconds(1);
            return Client.Connect(async error =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                if (++attempt < maxAttempts)
                {
                    _logger.LogWarning(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    _logger.LogError(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);

                    return false;
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Client.Close();
            }
            catch (OrleansException error)
            {
                _logger.LogWarning(error, "Error while gracefully disconnecting from Orleans cluster. Will ignore and continue to shutdown.");
            }
        }
    }
}
