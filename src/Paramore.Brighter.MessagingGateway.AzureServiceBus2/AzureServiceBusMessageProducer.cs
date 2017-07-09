using System;
using System.Threading.Tasks;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.Logging;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;

namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessageProducer : IAmAMessageProducerAsync
    {
        private static readonly Lazy<ILog> _logger = new Lazy<ILog>(LogProvider.For<AzureServiceBusMessageProducer>);

        private readonly MessageSenderPool _messageSenderPool;

        public AzureServiceBusMessageProducer(AzureServiceBusMessagingGatewayConfiguration config)
        {
            _messageSenderPool = new MessageSenderPool(config);
        }

        public Task SendAsync(Message message)
        {
            return SendWithDelayAsync(message);
        }

        public async Task SendWithDelayAsync(Message message, int delayMilliseconds = 0)
        {
            _logger.Value.DebugFormat("AzureServiceBusMessageProducer: Publishing message to topic {0}", message.Header.Topic);

            var messageSender = _messageSenderPool.Get(message.Header.Topic);

            var brokeredMessage = new BrokeredMessage(message.Body.Bytes)
            {
                MessageId = message.Id.ToString(),
                CorrelationId = message.Header.CorrelationId.ToString(),
                ContentType = message.Header.ContentType,
                SessionId = Guid.NewGuid().ToString(), // todo no idea if this is legit
                ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(delayMilliseconds),
                UserProperties =
                {
                    { BrighterHeaders.Topic, message.Header.Topic },
                    { BrighterHeaders.MessageType, (int)message.Header.MessageType },
                }
            };

            await messageSender.SendAsync(brokeredMessage).ConfigureAwait(false);

            _logger.Value.DebugFormat("AzureServiceBusMessageProducer: Published message with id {0} to topic '{1}' with a delay of {2}ms ",
                message.Id, message.Header.Topic, delayMilliseconds);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AzureServiceBusMessageProducer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // todo wait is probably bad
                _messageSenderPool?.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}