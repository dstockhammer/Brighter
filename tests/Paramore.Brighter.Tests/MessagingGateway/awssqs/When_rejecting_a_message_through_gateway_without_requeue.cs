using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using FluentAssertions;
using Paramore.Brighter.MessagingGateway.AWSSQS;
using Xunit;

namespace Paramore.Brighter.Tests.MessagingGateway.AWSSQS
{
    [Trait("Category", "AWS")]
    public class SqsMessageConsumerInvalidMessageTests : IDisposable
    {
        private readonly string _queueUrl = "https://sqs.eu-west-1.amazonaws.com/027649620536/TestSqsTopicQueue";
        private readonly TestAWSQueueListener _testQueueListener;
        private readonly IAmAMessageProducer _sender;
        private readonly IAmAMessageConsumer _receiver;
        private readonly Message _message;
        private Message _listenedMessage;

        public SqsMessageConsumerInvalidMessageTests()
        {
            var messageHeader = new MessageHeader(Guid.NewGuid(), "TestSqsTopic", MessageType.MT_COMMAND);

            messageHeader.UpdateHandledCount();
            _message = new Message(messageHeader, new MessageBody("test content"));

            var credentials = new AnonymousAWSCredentials();
            _sender = new SqsMessageProducer(credentials);
            _receiver = new SqsMessageConsumer(credentials, _queueUrl);
            _testQueueListener = new TestAWSQueueListener(credentials, _queueUrl);

            _sender.Send(_message);
        }

        [Fact]
        public async Task When_rejecting_a_message_through_gateway_without_requeue()
        {
            _listenedMessage = await _receiver.ReceiveAsync(1000);
            await _receiver.RejectAsync(_listenedMessage, false);

            //should_not_requeue_the_message
            _testQueueListener.Listen().Should().BeNull();
        }

        public void Dispose()
        {
            _testQueueListener.DeleteMessage(_listenedMessage.Header.Bag["ReceiptHandle"].ToString());
        }
    }
}