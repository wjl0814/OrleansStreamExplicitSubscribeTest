using Orleans;
using System.Threading.Tasks;

namespace Interfaces
{
    /// <summary>
    /// 向Stream 发送消息
    /// </summary>
    public interface IPublisherGrain : IGrainWithGuidKey
    {
        /// <summary>
        /// 向Stream发生消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task PublishMessageAsync(string data);
    }
}