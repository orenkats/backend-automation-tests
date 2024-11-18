using Application.Messaging;
using NUnit.Framework;

namespace Tests.Messaging;

[TestFixture]
public class RabbitMqTests
{
    private RabbitMqPublisher _publisher;
    private RabbitMqConsumer _consumer;

    [SetUp]
    public void Setup()
    {
        _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
        _consumer = new RabbitMqConsumer("localhost", "guest", "guest");
    }

    [Test]
    public async Task PublishAndConsumeTest()
    {
        string testQueue = "test_queue";
        string testMessage = "Hello, RabbitMQ!";
        string receivedMessage = null;

        // Start consuming
        await _consumer.StartConsumingAsync(testQueue, async (msg) =>
        {
            receivedMessage = (string)msg;

        // Publish a message
        await _publisher.PublishAsync(testQueue, testMessage);

        // Wait to ensure the message is consumed
        await Task.Delay(1000);

        Assert.AreEqual(testMessage, receivedMessage);
    }
}
