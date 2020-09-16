using Orleans;
using System.Threading.Tasks;

namespace SiloHost.HealthCheck.Interfaces
{
    public interface ILocalHealthCheckGrain : IGrainWithGuidKey
    {
        Task PingAsync();
    }
}