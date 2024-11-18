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
    public class QueueRequest
    {
        public string QueueName { get; set; }
        public OrderMessage? Message { get; set; } // Optional: Only needed for "send" endpoint
    }

    public class OrderMessage
    {
        public Guid TraderId { get; set; }
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string OrderType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] QueueRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName) || request.Message == null)
            return BadRequest("Queue name and message cannot be empty.");

        try
        {
            Console.WriteLine($"SendMessage called with queueName: {request.QueueName}, message: {request.Message}");
            await _publisher.PublishAsync(request.QueueName, request.Message);
            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
            return StatusCode(500, $"Error sending message: {ex.Message}");
        }
    }

    [HttpPost("receive")]
    public async Task<IActionResult> StartReceiving([FromBody] QueueRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName))
            return BadRequest("Queue name cannot be empty.");

        try
        {
            Console.WriteLine($"StartReceiving called for queueName: {request.QueueName}");
            await _consumer.StartConsumingAsync(request.QueueName, async (msg) =>
            {
                Console.WriteLine($"[x] Received message: {msg}");
            });

            return Ok($"Started listening to the queue: {request.QueueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartReceiving: {ex.Message}");
            return StatusCode(500, $"Error starting queue consumer: {ex.Message}");
        }
    }
}
