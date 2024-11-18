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
        if (string.IsNullOrEmpty(queueName) || string.IsNullOrEmpty(message))
            return BadRequest("Queue name and message cannot be empty.");

        try
        {
            await _publisher.PublishAsync(queueName, message);
            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending message: {ex.Message}");
        }
    }

    [HttpPost("receive")]
    public async Task<IActionResult> StartReceiving(string queueName)
    {
        if (string.IsNullOrEmpty(queueName))
            return BadRequest("Queue name cannot be empty.");

        try
        {
            await _consumer.StartConsumingAsync(queueName, async (msg) =>
            {
                Console.WriteLine($"Received: {msg}");
            });

            return Ok("Started listening to the queue.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error starting queue consumer: {ex.Message}");
        }
    }

}
