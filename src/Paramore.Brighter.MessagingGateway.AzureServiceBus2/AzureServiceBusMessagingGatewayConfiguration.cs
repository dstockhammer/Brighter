namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessagingGatewayConfiguration
    {
        public string Namespace { get; set; }

        public SharedAccessPolicy SharedAccessPolicy { get; set; }
    }

    public class SharedAccessPolicy
    {
        public string Name { get; set; }

        public string Key { get; set; }
    }
}
