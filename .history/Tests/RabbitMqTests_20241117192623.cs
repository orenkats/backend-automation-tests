using Application.Messaging;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests.Messaging
{
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

            var consumingTask = _consumer.StartConsumingAsync(testQueue, async (msg) =>
            {
                receivedMessage = msg; // Store the message for comparison
                await Task.CompletedTask; // Return a completed task
            });

            await _publisher.PublishAsync(testQueue, testMessage);

            // Wait for the consumer to process the message
            await Task.Delay(1000);

            Assert.AreEqual(testMessage, receivedMessage);
        }
    }
}
