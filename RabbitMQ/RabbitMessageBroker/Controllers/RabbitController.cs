using Microsoft.AspNetCore.Mvc;
using RabbitMessageBroker.Models;
using RabbitMessageBroker.RabbitMQ;

namespace RabbitMessageBroker.Controllers
{
    public class PublishRequest
    {
        public string Message { get; set; }
        public byte Priority { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class RabbitController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMessageBroker _broker;

        public RabbitController(IConfiguration config, IMessageBroker broker)
        {
            _config = config;
            _broker = broker;
        }

        [HttpPost("publish/exchange/{exchangeString}")]
        public async Task<IActionResult> PublishExchange(string exchangeString, [FromBody] PublishRequest message)
        {
            // Get Exchange
            List<Exchange>? exchanges = _config.GetSection("Rabbit:Exchanges").Get<List<Exchange>>();
            Exchange? exchange = exchanges?.FirstOrDefault(e => e.Name == exchangeString);

            // Publish to Exchange
            if (exchange != null)
            {
                await _broker.PublishAsync(exchange.Name, message.Priority, message.Message);
                return Ok($"Sent to Rabbit with priority {message.Priority}: {message.Message}");
            }

            return BadRequest("Exchange not found");
        }
    }
}