using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;

namespace SimpleClientTests;

public class ProducerConsumerTests
{
    private static readonly Uri connectUri = new Uri("activemq:tcp://localhost:61616");
    // private static readonly Uri _ranmdomFailover = new Uri("failover:(tcp://localhost:61616,tcp://localhost:61617,tcp://localhost:61618)?randomize=true");
    private static readonly string _username = "artemis";
    private static readonly string _password = "test";

    private static TimeSpan _timeout = TimeSpan.FromSeconds(10);
    private IConnectionFactory _factory = new ConnectionFactory(connectUri);

    [Test]
    public void CanProduceMessage()
    {
        using var connection = _factory.CreateConnection(_username, _password);
        using var session = connection.CreateSession();
        // var destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
        var destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
        using var producer = session.CreateProducer(destination);

        connection.Start();
        producer.DeliveryMode = MsgDeliveryMode.Persistent;
        producer.RequestTimeout = _timeout;

        for (int i = 0; i < 10; i++) 
        {
            var request = session.CreateTextMessage($"Hello World! #{i}");
            request.NMSCorrelationID = "abc";
            request.Properties["NMSXGroupID"] = "cheese";
            request.Properties["myHeader"] = "Cheddar";

            producer.Send(request);
        }

        Assert.Pass();
    }

    [Test]
    public void CanConsumeMessage()
    {
        using var connection = _factory.CreateConnection(_username, _password);
        using var session = connection.CreateSession();
        // var destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
        var destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
        using var consumer = session.CreateConsumer(destination);

        connection.Start();

        Console.WriteLine("Start receiving messages...");
        
        while (true)
        {
            ITextMessage message = (ITextMessage)consumer.Receive(_timeout);

            if (message == null)
            {
                Console.WriteLine("No more messages received!");
                break;
            }
            else
            {
                // Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                Console.WriteLine("Received message with text: " + message.Text);
            }
        }

        Assert.Pass();
    }
}