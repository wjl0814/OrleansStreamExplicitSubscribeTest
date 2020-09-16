using System;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace API.Controllers
{
    [Produces("application/json")]
    [Route("api/message")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly IClusterClient _client;

        public MessageController(IClusterClient client)
        {
            _client = client;
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SendMessage([FromBody] Message message)
        {
            await _client.GetGrain<IPublisherGrain>(Guid.Empty).PublishMessageAsync(message.MessageData);
            return Ok();
        }
    }

    public class Message
    {
        public string MessageData { get; set; }
    }
}
