using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Protoreg;
using RabbitMQ.Client;
using RabbitMQTest.Cronus.Core.Eventing;
using RabbitMQTest.LaCore.Hyperion.Infrastructure;

namespace RabbitMQTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var reg = new ProtoRegistration();
            reg.RegisterAssembly<TestMessage>();
            var serializer = new ProtoregSerializer(reg);
            serializer.Build();



            Thread trd = new Thread(new ThreadStart(() =>
            {
                using (var queue = new PersistentQueue<IEvent>("test-persistent-queue", serializer))
                {
                    while (true)
                    {

                        IEvent result;
                        if (queue.TryGetEvent(out result))
                        {
                            Console.WriteLine((result as TestMessage).TestString);
                            queue.Acknowlidge(result);
                        }
                    }
                }
            }));
            trd.Start();

            Thread trdPub = new Thread(new ThreadStart(() =>
            {
                var queuePub = new PersistentQueue<IEvent>("test-persistent-queue", serializer);

                int i = 0;
                while (true)
                {
                    string msgString = (i++).ToString();
                    if (String.IsNullOrEmpty(msgString))
                        continue;
                    if (msgString == "exit")
                        break;
                    var msg = new TestMessage(msgString);
                    queuePub.Enqueue(msg);

                } queuePub.Dispose();
            }));
            trdPub.Start();

            Console.ReadLine();
            //var connectionFactory = new ConnectionFactory();
            //IConnection connection = connectionFactory.CreateConnection();
            //byte[] message = Encoding.UTF8.GetBytes("Test Message");

            //var start = DateTime.Now;
            //IModel channel = connection.CreateModel();
            //for (int i = 0; i < 8000; i++)
            //{

            //    //PublicationAddress adress = new PublicationAddress(string.Empty, null, "test-queue");

            //    //  channel.BasicPublish(adress, null, message);
            //    var queue = channel.QueueDeclare("test-queue", true, false, false, null);
            //    var result = channel.BasicGet("test-queue", false);

            //    //  channel.BasicAck(result.DeliveryTag, false);


            //}
            //channel.Close();
            //var end = DateTime.Now; Console.WriteLine(end - start);

            //connection.Close();

            //Console.ReadLine();
        }



    }
    [DataContract(Name = "1ed4f3c5-5fff-4289-94f5-a5cfd9379734")]
    public class TestMessage : IEvent
    {
        public TestMessage() { }
        public TestMessage(string testString)
        {
            TestString = testString;
        }
        [DataMember(Order = 1)]
        public string TestString { get; set; }
    }

    namespace Cronus.Core.Eventing
    {
        public interface IEvent
        { }

    }
    namespace LaCore.Hyperion.Infrastructure
    {
        public class PersistentQueue<T> : IDisposable
            where T : class
        {
            public const string CrojectionsQueueFormatName = "rabbitmq://localhost/Hyperion-{0}-Projections";
            private IConnection connection;
            private ConnectionFactory connectionFactory;
            private IModel errorChannel;
            private QueueDeclareOk queue;
            private readonly string queueName;
            private readonly string errorQueueName;
            private ConcurrentQueue<T> readQueue = new ConcurrentQueue<T>();
            private Dictionary<T, BasicGetResult> results = new Dictionary<T, BasicGetResult>();
            private IModel channel;
            private readonly ProtoregSerializer serializer;

            public PersistentQueue(string name, ProtoregSerializer serializer)
            {
                this.serializer = serializer;
                if (connection == null)
                {
                    if (connectionFactory == null)
                    {
                        connectionFactory = new ConnectionFactory();
                        connection = connectionFactory.CreateConnection();
                    }
                }
                this.queueName = name;
                this.errorQueueName = name + "_error";
                channel = connection.CreateModel();
                queue = channel.QueueDeclare(queueName, true, false, false, null);
                errorChannel = connection.CreateModel();
                errorChannel.QueueDeclare(errorQueueName, true, false, false, null);
            }

            public void Acknowlidge(T item)
            {
                channel.BasicAck(results[item].DeliveryTag, false);
                results.Remove(item);
            }

            public void AcknowlidgeRange(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    Acknowlidge(item);
                }
            }

            public T Dequeue()
            {
                var result = channel.BasicGet(queueName, false);
                if (result == null)
                    return null;
                var str = new MemoryStream(result.Body);
                str.Position = 0;
                if (result == null || result.Body == null || result.Body.Length == 0)
                    return null;
                object item = null; ;
                try
                {
                    throw new Exception();
                    item = serializer.Deserialize(str);
                }
                catch (Exception ex)
                {
                    errorChannel.BasicPublish(String.Empty, errorQueueName, null, result.Body);
                    channel.BasicAck(result.DeliveryTag, false);
                    //  throw ex;
                    return null;
                }
                results.Add((T)item, result);
                return (T)item;
            }

            public void Enqueue(T item)
            {
                var str = new MemoryStream();
                serializer.Serialize(str, item);
                channel.BasicPublish(String.Empty, queueName, null, str.ToArray());
            }

            public void Dispose()
            {
                if (channel != null)
                {
                    channel.Close();
                    channel = null;
                }
                if (connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }

            public bool TryGetEvent(out Cronus.Core.Eventing.IEvent @event)
            {

                @event = Dequeue() as IEvent;
                if (@event == null)
                    return false;
                return true;
            }

            public bool IsEmpty
            {
                get
                {
                    return queue.MessageCount == 0;
                }
            }

            public int Count
            {
                get
                {
                    if (queue.MessageCount > int.MaxValue)
                        return int.MaxValue;
                    else
                        return (int)queue.MessageCount;
                }
            }
        }
    }

}
