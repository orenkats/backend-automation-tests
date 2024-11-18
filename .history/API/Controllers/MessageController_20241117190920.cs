using Application.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;
    private readonly RabbitMqConsumer _consumer;

    public MessageController(RabbitMqPublisher publisher, RabbitMqConsumer consumer)
    {
        _publisher = publisher;
        _consumer = consumer;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(string queueName, string message)
    {
        await _publisher.PublishAsync(queueName, message);
        return Ok("Message sent successfully.");
    }

    [HttpPost("receive")]
    public IActionResult StartReceiving(string queueName)
    {
        _consumer.StartConsumingAsync(queueName, async (msg) =>
        {
            Console.WriteLine($"Received: {msg}");
        });

        return Ok("Started listening to the queue.");
    }
}
