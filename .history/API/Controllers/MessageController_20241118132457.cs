using Application.Messaging;
using Domain.Entities; 
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

    // Model for the queue request, including queueName and StockOrder
    public class QueueRequest
    {
        public string QueueName { get; set; } = null!;
        public StockOrder Order { get; set; } = null!;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] QueueRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName) || request.Order == null || 
            request.Order.TraderId == Guid.Empty || string.IsNullOrEmpty(request.Order.StockSymbol))
        {
            return BadRequest("Queue name, trader ID, and order details are required.");
        }

        try
        {
            Console.WriteLine($"SendMessage called for queue: {request.QueueName}, TraderId: {request.Order.TraderId}, StockSymbol: {request.Order.StockSymbol}");
            await _publisher.PublishAsync(request.QueueName, request.Order);
            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending message: {ex.Message}");
        }
    }

    [HttpPost("receive")]
    public async Task<IActionResult> StartReceiving([FromBody] string queueName)
    {
        if (string.IsNullOrEmpty(queueName))
            return BadRequest("Queue name cannot be empty.");

        try
        {
            Console.WriteLine($"StartReceiving called for queue: {queueName}");
            await _consumer.StartConsumingAsync(queueName, async (msg) =>
            {
                Console.WriteLine($"[x] Received message: {msg}");
            });

            return Ok($"Started listening to the queue: {queueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartReceiving: {ex.Message}");
            return StatusCode(500, $"Error starting queue consumer: {ex.Message}");
        }
    }
}
