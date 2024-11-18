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
        string testQueue = "test_queue";
        string testMessage = "Hello, RabbitMQ!";
        string receivedMessage = null;

        var consumingTask = _consumer.StartConsumingAsync(testQueue, async (msg) =>
        {
            receivedMessage = msg;
            await Task.CompletedTask;
        });

        await _publisher.PublishAsync(testQueue, testMessage);
        await Task.Delay(1500);

        Assert.Equal(testMessage, receivedMessage);
    }
}
