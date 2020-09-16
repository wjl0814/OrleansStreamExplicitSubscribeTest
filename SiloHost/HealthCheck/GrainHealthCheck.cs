using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using SiloHost.HealthCheck.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiloHost.HealthCheck
{
    public class GrainHealthCheck : IHealthCheck
    {
        private readonly IClusterClient _client;

        public GrainHealthCheck(IClusterClient client)
        {
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.GetGrain<ILocalHealthCheckGrain>(Guid.Empty).PingAsync();
            }
            catch (Exception error)
            {
                return HealthCheckResult.Unhealthy("Failed to ping the local health check grain.", error);
            }
            return HealthCheckResult.Healthy();
        }
    }
}