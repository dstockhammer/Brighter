using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessageConsumer : IAmAMessageConsumerSupportingDelay
    {
        public bool DelaySupported => true;

        private readonly IMessageReceiver _messageReceiver;

        public AzureServiceBusMessageConsumer(AzureServiceBusMessagingGatewayConfiguration config, string queueName, string routingKey, bool isDurable, ushort preFetchSize = 1, bool highAvailability = false)
        {
            var csb = new ServiceBusConnectionStringBuilder(config.Namespace, queueName, config.SharedAccessPolicy.Name, config.SharedAccessPolicy.Key);
            _messageReceiver = new MessageReceiver(csb);
        }

        public async Task<Message> ReceiveAsync(int timeoutInMilliseconds)
        {
            var brokeredMessage = await _messageReceiver.ReceiveAsync(TimeSpan.FromMilliseconds(timeoutInMilliseconds)).ConfigureAwait(false);
            if (brokeredMessage == null)
                return new Message();

            var header = new MessageHeader(
                Guid.Parse(brokeredMessage.MessageId),
                (string)brokeredMessage.UserProperties[BrighterHeaders.Topic],
                (MessageType)brokeredMessage.UserProperties[BrighterHeaders.MessageType],
                brokeredMessage.SystemProperties.EnqueuedTimeUtc,
                brokeredMessage.CorrelationId == null ? (Guid?) null : Guid.Parse(brokeredMessage.CorrelationId),
                brokeredMessage.ReplyTo,
                brokeredMessage.ContentType);

            if (brokeredMessage.SystemProperties.IsLockTokenSet)
            {
                header.Bag.Add("LockToken", brokeredMessage.SystemProperties.LockToken);
            }

            var body = new MessageBody(brokeredMessage.Body, brokeredMessage.ContentType); // todo content type is probably wrong

            return new Message(header, body);
        }

        public async Task AcknowledgeAsync(Message message)
        {
            if (message.Header.Bag.ContainsKey("LockToken"))
            {
                var lockToken = (string)message.Header.Bag["LockToken"];
                await _messageReceiver.CompleteAsync(lockToken).ConfigureAwait(false);
            }
        }

        public async Task RejectAsync(Message message, bool requeue)
        {
            if (message.Header.Bag.ContainsKey("LockToken"))
            {
                var lockToken = (string)message.Header.Bag["LockToken"];
                await _messageReceiver.DeadLetterAsync(lockToken).ConfigureAwait(false); // todo not sure about dead letter
            }
        }

        public Task PurgeAsync()
        {
            throw new NotImplementedException();
        }

        public Task RequeueAsync(Message message)
        {
            return RequeueAsync(message, 0);
        }

        public async Task RequeueAsync(Message message, int delayMilliseconds)
        {
            // todo delay
            if (message.Header.Bag.ContainsKey("LockToken"))
            {
                var lockToken = (string)message.Header.Bag["LockToken"];
                await _messageReceiver.DeferAsync(lockToken).ConfigureAwait(false); // todo not sure about defer
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AzureServiceBusMessageConsumer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // todo wait is probably bad
                _messageReceiver?.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}
