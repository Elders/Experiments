using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Protoreg;
using RabbitMQ.Client;

namespace RabbitMqRepository
{
    class Program
    {
        static void Main(string[] args)
        {


        }
        public class RabbitRepository
        {
            PersistentQueue<TransactionalObject> queue;
            static ConcurrentDictionary<object, TransactionalObject> allObjects = new ConcurrentDictionary<object, TransactionalObject>();
            public RabbitRepository(string repositoryName, ProtoregSerializer serializer)
            {
                this.repositoryName = repositoryName;
                this.serializer = serializer;
                queue = new PersistentQueue<TransactionalObject>(repositoryName, serializer);

            }
            
            string repositoryName;

            ProtoregSerializer serializer;

            
            public void Save(object scope, object item)
            {
                queue.Enqueue(new TransactionalObject(item, Guid.NewGuid(), 0));
            }

            
        }
        [DataContract(Name = "8c5f1d4b-e33b-4aeb-b72e-139ff03dfad0")]
        public class TransactionalObject
        {
            public TransactionalObject() { }

            public TransactionalObject(object item, Guid transactionalObjectId, int transactionNumber)
            {
                Item = item;
                TransactionalObjectId = transactionalObjectId;
                TransactionNumber = transactionNumber;
            }
            [DataMember(Order = 1)]
            public object Item { get; set; }

            [DataMember(Order = 2)]
            public Guid TransactionalObjectId { get; set; }

            [DataMember(Order = 3)]
            public int TransactionNumber { get; set; }
        }
    }

    public class PersistentQueue<T> : IDisposable
           where T : class
    {
        public const string CrojectionsQueueFormatName = "rabbitmq://localhost/Hyperion-{0}-Projections";
        private static IConnection connection;
        private static ConnectionFactory connectionFactory;
        private QueueDeclareOk queue;
        private readonly string queueName;
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
            channel = connection.CreateModel();
            queue = channel.QueueDeclare(queueName, true, false, false, null);
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
            var item = serializer.Deserialize(str);
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
    }
}
