using Orleans;
using Orleans.Concurrency;
using SiloHost.HealthCheck.Interfaces;
using System.Threading.Tasks;

namespace SiloHost.HealthCheck.Grains
{
    [StatelessWorker(1)]
    public class LocalHealthCheckGrain : Grain, ILocalHealthCheckGrain
    {
        public Task PingAsync() => Task.CompletedTask;
    }
}