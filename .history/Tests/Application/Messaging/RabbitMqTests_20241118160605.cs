using Application.Messaging;
using Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Tests.Common.Helpers;

namespace Tests.Messaging
{
    [TestFixture]
    public class RabbitMqTests
    {
        private RabbitMqPublisher _publisher;
        private RabbitMqConsumer _consumer;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            // Set up dependency injection with an in-memory database and scope factory
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddSingleton<RabbitMqPublisher>();
            services.AddSingleton<RabbitMqConsumer>();

            _serviceProvider = services.BuildServiceProvider();

            // Resolve RabbitMqPublisher and RabbitMqConsumer
            _publisher = _serviceProvider.GetRequiredService<RabbitMqPublisher>();
            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _consumer = new RabbitMqConsumer(scopeFactory);
        }

        [Test]
        public async Task PublishAndConsumeTest()
        {
            // Arrange: Define the test queue and message
            string testQueue = "test_queue";
            string testMessage = "Hello, RabbitMQ!";
            string receivedMessage = null;

            // Act: Start the consumer and publish a message
            var consumingTask = _consumer.StartConsumingAsync(testQueue, async (msg) =>
            {
                receivedMessage = msg; // Capture the received message
                Console.WriteLine($"[Test Consumer] Received: {msg}");
                await Task.CompletedTask; // Complete the callback task
            });

            await _publisher.PublishAsync(testQueue, testMessage);
            Console.WriteLine($"[Test Publisher] Sent: {testMessage}");

            // Wait to ensure the message is processed
            await Task.Delay(1500);

            // Assert: Validate that the message was received correctly
            Assert.AreEqual(testMessage, receivedMessage);

            Console.WriteLine("[Test] Message successfully published and consumed.");
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the service provider to clean up resources
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
