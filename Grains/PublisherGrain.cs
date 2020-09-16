using System.Threading.Tasks;
using Interfaces;
using Orleans;
using Orleans.Streams;

namespace Grains
{
    /// <summary>
    /// 向Stream 发送消息
    /// </summary>
    public class PublisherGrain : Grain, IPublisherGrain
    {
        private IAsyncStream<string> _stream;

        /// <summary>
        /// Grain 激活
        /// </summary>
        /// <returns></returns>
        public override Task OnActivateAsync()
        {
            var streamId = this.GetPrimaryKey();
            var streamProvider = GetStreamProvider("SMSProvider");
            _stream = streamProvider.GetStream<string>(streamId, "GrainExplicitStream");
            return base.OnActivateAsync();
        }

        /// <summary>
        /// 向Stream发生消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task PublishMessageAsync(string data) => await _stream.OnNextAsync(data);
    }
}
