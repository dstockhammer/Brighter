namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessageConsumerFactory : IAmAMessageConsumerFactory
    {
        private readonly AzureServiceBusMessagingGatewayConfiguration _config;

        public AzureServiceBusMessageConsumerFactory(AzureServiceBusMessagingGatewayConfiguration config)
        {
            _config = config;
        }

        public IAmAMessageConsumer Create(string queueName, string routingKey, bool isDurable, ushort preFetchSize = 1, bool highAvailability = false)
        {
            return new AzureServiceBusMessageConsumer(_config, queueName, routingKey, isDurable, preFetchSize, highAvailability);
        }
    }
}
