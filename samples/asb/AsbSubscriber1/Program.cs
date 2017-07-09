using System;
using Greetings.Adapters.ServiceHost;
using Greetings.Ports.CommandHandlers;
using Greetings.Ports.Commands;
using Greetings.Ports.Mappers;
using Greetings.TinyIoc;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;
using Paramore.Brighter.ServiceActivator;
using Serilog;

namespace AsbSubscriber1
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var container = new TinyIoCContainer();

            var handlerFactory = new TinyIocHandlerFactory(container);
            var messageMapperFactory = new TinyIoCMessageMapperFactory(container);
            container.Register<IHandleRequests<GreetingEvent>, GreetingEventHandler>();
            container.Register<IHandleRequests<GreetACommand>, GreetingACommandHandler>();

            var subscriberRegistry = new SubscriberRegistry();
            subscriberRegistry.Register<GreetingEvent, GreetingEventHandler>();
            subscriberRegistry.Register<GreetACommand, GreetingACommandHandler>();

            var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory);
            messageMapperRegistry.Register<GreetingEvent, GreetingEventMessageMapper>();
            messageMapperRegistry.Register<GreetACommand, GreetACommandMessageMapper>();

            var asbConfig = new AzureServiceBusMessagingGatewayConfiguration
            {
                Namespace = "darker.servicebus.windows.net",
                SharedAccessPolicy = new SharedAccessPolicy
                {
                    Name = "RootManageSharedAccessKey",
                    Key = "nope"
                }
            };

            var messageConsumerFactory = new AzureServiceBusMessageConsumerFactory(asbConfig);

            var dispatcher = DispatchBuilder.With()
                .CommandProcessor(CommandProcessorBuilder.With()
                    .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
                    .DefaultPolicy()
                    .NoTaskQueues()
                    .InMemoryRequestContextFactory()
                    .Build())
                .MessageMappers(messageMapperRegistry)
                .DefaultChannelFactory(new InputChannelFactory(messageConsumerFactory))
                .Connections(new Connection[]
                {
                    new Connection<GreetingEvent>(
                        new ConnectionName("paramore.example.greeting"),
                        new ChannelName("greeting.event/subscriptions/greeting.event.a"),
                        new RoutingKey("greeting.event"), // todo this doesn't do anything
                        timeoutInMilliseconds: 200),
                    new Connection<GreetACommand>(
                        new ConnectionName("paramore.example.greeting.a"),
                        new ChannelName("greeting.a.command"),
                        new RoutingKey("greeting.a.command"), // todo this doesn't do anything
                        timeoutInMilliseconds: 200)
                }).Build();

            dispatcher.Receive();

            Console.WriteLine("Press Enter to stop ...");
            Console.ReadKey();

            dispatcher.End().Wait();
        }
    }
}