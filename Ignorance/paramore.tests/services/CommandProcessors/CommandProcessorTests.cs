﻿using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Machine.Specifications;
using Paramore.Services.CommandHandlers;
using Paramore.Services.CommandProcessors;
using Paramore.Tests.services.CommandProcessors.TestDoubles;

namespace Paramore.Tests.services.CommandProcessors
{
    [Subject("Basic send of a command")]
    public class When_sending_a_command_to_the_processor
    {
        static CommandProcessor commandProcessor;
        static readonly MyCommand myCommand = new MyCommand();

        Establish context = () =>
        {
            var container = new WindsorContainer();
            container.Register(Component.For<IHandleRequests<MyCommand>>().ImplementedBy<MyCommandHandler>());
            
            commandProcessor = new CommandProcessor(container);

        };

        Because of = () => commandProcessor.Send(myCommand);

        It should_send_the_command_to_the_command_handler = () => MyCommandHandler.ShouldRecieve(myCommand).ShouldBeTrue();
    }

    [Subject("Commands should only have one handler")]
    public class When_there_are_multiple_possible_command_handlers
    {
        static CommandProcessor commandProcessor;
        static readonly MyCommand myCommand = new MyCommand();
        private static Exception exception;

        Establish context = () =>
        {
            var container = new WindsorContainer();
            container.Register(Component.For<IHandleRequests<MyCommand>>().ImplementedBy<MyCommandHandler>());
            container.Register(Component.For<IHandleRequests<MyCommand>>().ImplementedBy<MyImplicitHandler>());
            
            commandProcessor = new CommandProcessor(container);

        };

        Because of = () =>  exception = Catch.Exception(() => commandProcessor.Send(myCommand));

        It should_fail_because_multiple_recievers_found = () => exception.ShouldBeOfType(typeof (ArgumentException));
        It should_have_an_error_message_that_tells_you_why = () => exception.ShouldContainErrorMessage("More than one handler was found for the typeof command Paramore.Tests.services.CommandProcessors.TestDoubles.MyCommand - a command should only have one handler.");
    }

    [Subject("Commands should have at least one handler")]
    public class When_there_are_no_command_handlers
    {
        static CommandProcessor commandProcessor;
        static readonly MyCommand myCommand = new MyCommand();
        private static Exception exception;

        Establish context = () =>
        {
            var container = new WindsorContainer();
            commandProcessor = new CommandProcessor(container);

        };

        Because of = () =>  exception = Catch.Exception(() => commandProcessor.Send(myCommand));

        It should_fail_because_multiple_recievers_found = () => exception.ShouldBeOfType(typeof (ArgumentException));
        It should_have_an_error_message_that_tells_you_why = () => exception.ShouldContainErrorMessage("No command handler was found for the typeof command Paramore.Tests.services.CommandProcessors.TestDoubles.MyCommand - a command should have only one handler."); 
    }

    [Subject("Basic event publishing")]
    public class When_publishing_an_event_to_the_processor
    {
        static CommandProcessor commandProcessor;
        static readonly MyEvent myEvent = new MyEvent();

        Establish context = () =>
        {
            var container = new WindsorContainer();
            container.Register(Component.For<IHandleRequests<MyEvent>>().ImplementedBy<MyEventHandler>());
            
            commandProcessor = new CommandProcessor(container);
        };

        Because of = () => commandProcessor.Publish(myEvent);

        It should_publish_the_command_to_the_event_handlers = () => MyEventHandler.ShouldRecieve(myEvent).ShouldBeTrue();
    }

    [Subject("An event may have no subscribers")]
    public class When_there_are_no_subscribers
    {
        static CommandProcessor commandProcessor;
        static readonly MyEvent myEvent = new MyEvent();
        static Exception exception;

        Establish context = () =>
        {
            commandProcessor = new CommandProcessor(new WindsorContainer());                                    
        };

        Because of = () => exception = Catch.Exception(() => commandProcessor.Publish(myEvent));

        It should_not_throw_an_exception = () => exception.ShouldBeNull();
    }

    [Subject("An event with multiple subscribers")]
    public class When_there_are_multiple_subscribers
    {
        static CommandProcessor commandProcessor;
        static readonly MyEvent myEvent = new MyEvent();
        static Exception exception;

        Establish context = () =>
                                {
                                    IWindsorContainer container = new WindsorContainer();
                                    container.Register(Component.For<IHandleRequests<MyEvent>>().ImplementedBy<MyEventHandler>());
                                    container.Register(Component.For<IHandleRequests<MyEvent>>().ImplementedBy<MyOtherEventHandler>());
                                    commandProcessor = new CommandProcessor(container);
                                };

        Because of = () => exception = Catch.Exception(() => commandProcessor.Publish(myEvent));

        It should_not_throw_an_exception = () => exception.ShouldBeNull();
        It should_publish_the_command_to_the_first_event_handler = () => MyEventHandler.ShouldRecieve(myEvent).ShouldBeTrue(); 
        It should_publish_the_command_to_the_second_event_handler = () => MyOtherEventHandler.ShouldRecieve(myEvent).ShouldBeTrue();}
}