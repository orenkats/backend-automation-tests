using Application.Messaging;
using System.Threading.Tasks;
using Xunit;

public class RabbitMqTests
{
    private RabbitMqPublisher _publisher;
    private RabbitMqConsumer _consumer;

    public RabbitMqTests()
    {
        // Setup RabbitMqPublisher and RabbitMqConsumer for testing
        _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
        var dbContext = TestHelper.CreateInMemoryDbContext();
        _consumer = new RabbitMqConsumer(dbContext);
    }

    [Fact] // Correct xUnit attribute
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
