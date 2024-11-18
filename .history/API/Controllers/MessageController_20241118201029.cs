using Application.Messaging;
using Domain.Entities; 
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;
    private readonly RabbitMqConsumer _consumer;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public MessageController(RabbitMqPublisher publisher, RabbitMqConsumer consumer)
    {
        _publisher = publisher;
        _consumer = consumer;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    // Model for the queue request, including queueName and StockOrder
    public class QueueRequest
    {
        public string QueueName { get; set; } = null!;
        public StockOrder? Message { get; set; } = null!;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] QueueRequest request)
    {
        if (string.IsNullOrEmpty(request.QueueName) || request.Message == null || 
            request.Message.TraderId == Guid.Empty || string.IsNullOrEmpty(request.Message.StockSymbol))
        {
            return BadRequest("Queue name, trader ID, and order details are required.");
        }

        try
        {
            // Add timestamp if missing
            if (request.Message.CreatedAt == default)
            {
                request.Message.CreatedAt = DateTime.UtcNow;
            }

            Console.WriteLine($"SendMessage called for queue: {request.QueueName}, TraderId: {request.Message.TraderId}, StockSymbol: {request.Message.StockSymbol}");
            await _publisher.PublishAsync(request.QueueName, request.Message);
            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
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
            Console.WriteLine($"StartReceiving called for queue: {request.QueueName}");
            var consumingTask = _consumer.StartConsumingAsync(
                request.QueueName,
                async (msg) =>
                {
                    Console.WriteLine($"[x] Received message: {msg}");
                },
                _cancellationTokenSource.Token // Pass the cancellation token
            );

            return Ok($"Started listening to the queue: {request.QueueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartReceiving: {ex.Message}");
            return StatusCode(500, $"Error starting queue consumer: {ex.Message}");
        }
    }

    [HttpPost("stop")]
    public IActionResult StopReceiving()
    {
        try
        {
            Console.WriteLine("[*] Stopping the consumer...");
            _cancellationTokenSource.Cancel();
            return Ok("Consumer stopped successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping the consumer: {ex.Message}");
            return StatusCode(500, $"Error stopping queue consumer: {ex.Message}");
        }
    }
}
