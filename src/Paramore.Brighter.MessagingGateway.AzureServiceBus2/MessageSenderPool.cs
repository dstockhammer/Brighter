using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    internal sealed class MessageSenderPool
    {
        private readonly AzureServiceBusMessagingGatewayConfiguration _config;
        private readonly ConcurrentDictionary<string, MessageSender> _messageSenders;

        public MessageSenderPool(AzureServiceBusMessagingGatewayConfiguration config)
        {
            _config = config;
            _messageSenders = new ConcurrentDictionary<string, MessageSender>();
        }

        public MessageSender Get(string topic)
        {
            if (_messageSenders.TryGetValue(topic, out MessageSender existing))
                return existing;

            var csb = new ServiceBusConnectionStringBuilder(_config.Namespace, topic, _config.SharedAccessPolicy.Name, _config.SharedAccessPolicy.Key);
            var sender = new MessageSender(csb);

            // todo handle potential race
            _messageSenders.TryAdd(topic, sender);

            return sender;
        }

        public async Task CloseAsync()
        {
            var closeTasks = _messageSenders.Values.Select(s => s.CloseAsync());
            await Task.WhenAll(closeTasks).ConfigureAwait(false);
        }
    }
}