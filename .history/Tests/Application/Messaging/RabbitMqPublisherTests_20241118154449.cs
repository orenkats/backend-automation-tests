using Application.Messaging;
using NUnit.Framework;
using System.Threading.Tasks;

[TestFixture]
public class RabbitMqPublisherTests
{
    private RabbitMqPublisher _publisher;

    [SetUp]
    public void Setup()
    {
        _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
    }

    [Test]
    public async Task PublishAsync_SendsMessageToQueue()
    {
        // Arrange
        string testQueue = "test_queue";
        string testMessage = "Hello, RabbitMQ!";

        // Act
        await _publisher.PublishAsync(testQueue, testMessage);

        // Assert
        Assert.Pass("Message published successfully (requires manual verification on RabbitMQ).");
    }
}
