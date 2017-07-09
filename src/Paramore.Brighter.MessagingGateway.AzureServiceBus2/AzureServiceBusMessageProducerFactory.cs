namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessageProducerFactory : IAmAMessageProducerFactoryAsync
    {
        private readonly AzureServiceBusMessagingGatewayConfiguration _config;

        public AzureServiceBusMessageProducerFactory(AzureServiceBusMessagingGatewayConfiguration config)
        {
            _config = config;
        }

        public IAmAMessageProducerAsync Create()
        {
            return new AzureServiceBusMessageProducer(_config);
        }
    }
}