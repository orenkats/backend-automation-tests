using Application.Messaging;
using Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
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
            // Setup a service provider for the required dependencies
            var services = new ServiceCollection();
            
            // Use an in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            
            services.AddScoped<AppDbContext>();
            services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());

            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the IServiceScopeFactory
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            // Initialize RabbitMQ Publisher and Consumer
            _publisher = new RabbitMqPublisher("localhost", "guest", "guest");
            _consumer = new RabbitMqConsumer(serviceScopeFactory);
        }

        [Test]
        public async Task PublishAndConsumeTest()
        {
            // Define test queue and message
            string testQueue = "test_queue";
            string testMessage = "Hello, RabbitMQ!";
            string receivedMessage = null;

            // Start the consumer
            var consumingTask = _consumer.StartConsumingAsync(testQueue, async (msg) =>
            {
                receivedMessage = msg; // Capture the received message
                Console.WriteLine($"[Test Consumer] Received: {msg}");
                await Task.CompletedTask; // Signal task completion
            });

            // Publish a test message
            await _publisher.PublishAsync(testQueue, testMessage);
            Console.WriteLine($"[Test Publisher] Sent: {testMessage}");

            // Allow time for the message to be processed
            await Task.Delay(1500);

            // Assert that the received message matches the sent message
            Assert.AreEqual(testMessage, receivedMessage);

            Console.WriteLine("[Test] Message successfully published and consumed.");
        }
    }
}
