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

    // Model for sending messages
    public class SendMessageRequest
    {
        public string QueueName { get; set; } 
        public string Message { get; set; }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName) || string.IsNullOrEmpty(request.Message))
            return BadRequest("Queue name and message cannot be empty.");

        try
        {
            Console.WriteLine($"SendMessage called with queueName: {request.QueueName}, message: {request.Message}");
            await _publisher.PublishAsync(request.QueueName, request.Message);
            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending message: {ex.Message}");
        }
    }

    [HttpPost("receive")]
    public async Task<IActionResult> StartReceiving([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName))
            return BadRequest("Queue name cannot be empty.");

        try
        {
            await _consumer.StartConsumingAsync(request.QueueName, async (msg) =>
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
