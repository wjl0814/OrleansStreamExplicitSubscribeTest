using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;

namespace SiloHost
{
    public class StreamSubscribeWorker : BackgroundService
    {
        private readonly IClusterClient _client;
        private readonly ILogger<StreamSubscribeWorker> _logger;

        public StreamSubscribeWorker(IClusterClient client, ILogger<StreamSubscribeWorker> logger)
        {
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10000);
            var streamProvider = _client.GetStreamProvider("SMSProvider");
            IAsyncStream<string> stream = streamProvider.GetStream<string>(Guid.Empty, "GrainExplicitStream");
            await stream.SubscribeAsync((payload, token) => ReceivedMessageAsync(payload));
        }

        public Task ReceivedMessageAsync(string data)
        {
            _logger.LogInformation($"-------------------------------Received message:{data}----------------------------------");
            return Task.CompletedTask;
        }
    }
}
