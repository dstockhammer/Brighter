#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Paramore.Brighter.Tests.MessagingGateway
{
    public class ChannelMessageReceiveTests
    {
        private readonly IAmAChannel _channel;
        private readonly IAmAMessageConsumer _gateway;
        private Message _receivedMessage;
        private readonly Message _sentMessage;

        public ChannelMessageReceiveTests()
        {
            _gateway = A.Fake<IAmAMessageConsumer>();

            _channel = new Channel(new ChannelName("test"), _gateway);

            _sentMessage = new Message(
                new MessageHeader(Guid.NewGuid(), "key", MessageType.MT_EVENT),
                new MessageBody("a test body"));

            A.CallTo(() => _gateway.ReceiveAsync(1000)).Returns(_sentMessage);
        }

        [Fact]
        public async Task When_Listening_To_Messages_On_A_Channel()
        {
            _receivedMessage = await _channel.ReceiveAsync(1000);

            //_should_call_the_messaging_gateway
            A.CallTo(() => _gateway.ReceiveAsync(1000)).MustHaveHappened();
            //_should_return_the_next_message_from_the_gateway
            _receivedMessage.Should().Be(_sentMessage);
        }
    }
}