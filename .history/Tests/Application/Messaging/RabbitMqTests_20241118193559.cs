using Application.Messaging;
using Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Common;
using Xunit;

public class RabbitMqTests
{
    private RabbitMqPublisher _publisher;
    private RabbitMqConsumer _consumer;

    public RabbitMqTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<RabbitMqConsumer>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb")); // Use an in-memory database for testing
        var serviceProvider = services.BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _consumer = new RabbitMqConsumer(scopeFactory);
        _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
    }

    [Fact]
    public async Task PublishAndConsumeTest()
    {
        // Arrange
        string testQueue = "test_queue";
        string testMessage = "Hello, RabbitMQ!";
        var messageReceivedTcs = new TaskCompletionSource<string>();

        // Start the consumer
        var consumingTask = _consumer.StartConsumingAsync(testQueue, async (msg) =>
        {
            messageReceivedTcs.TrySetResult(msg); // Signal when the message is received
            await Task.CompletedTask;
        });

        // Publish a message
        await _publisher.PublishAsync(testQueue, testMessage);

        // Wait for the message to be received
        var receivedMessage = await messageReceivedTcs.Task;

        // Assert
        Assert.Equal(testMessage, receivedMessage);

        // Ensure the consumer stops gracefully
        await Task.WhenAny(consumingTask, Task.Delay(5000));
    }

}
