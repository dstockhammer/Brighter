using System;
using Greetings.Adapters.ServiceHost;
using Greetings.Ports.Commands;
using Greetings.Ports.Mappers;
using Greetings.TinyIoc;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;
using Serilog;

namespace AsbProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var container = new TinyIoCContainer();
            var messageMapperFactory = new TinyIoCMessageMapperFactory(container);
            var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory);
            messageMapperRegistry.Register<GreetingEvent, GreetingEventMessageMapper>();
            messageMapperRegistry.Register<GreetACommand, GreetACommandMessageMapper>();
            messageMapperRegistry.Register<GreetBCommand, GreetBCommandMessageMapper>();

            var asbConfig = new AzureServiceBusMessagingGatewayConfiguration
            {
                Namespace = "darker.servicebus.windows.net",
                SharedAccessPolicy = new SharedAccessPolicy
                {
                    Name = "RootManageSharedAccessKey",
                    Key = "nope"
                }
            };

            var producer = new AzureServiceBusMessageProducer(asbConfig);

            var cp = CommandProcessorBuilder.With()
                .NoHandlers()
                .DefaultPolicy()
                .TaskQueues(new MessagingConfiguration(new NoOpMessageStore(), producer, messageMapperRegistry))
                .InMemoryRequestContextFactory()
                .Build();

            Console.WriteLine("Press a | b | e | q");

            while (true)
            {
                var id = Guid.NewGuid();
                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'a':
                        Console.WriteLine("Posting command A with id {0}", id);
                        cp.PostAsync(new GreetACommand($"hello world {id}")).GetAwaiter().GetResult();
                        break;

                    case 'b':
                        Console.WriteLine("Posting command B with id {0}", id);
                        cp.PostAsync(new GreetBCommand($"hello world {id}")).GetAwaiter().GetResult();
                        break;

                    case 'e':
                        Console.WriteLine("Posting event with id {0}", id);
                        cp.PostAsync(new GreetingEvent($"hello world {id}")).GetAwaiter().GetResult();
                        break;

                    case 'q':
                        return;
                }
            }
        }
    }
}