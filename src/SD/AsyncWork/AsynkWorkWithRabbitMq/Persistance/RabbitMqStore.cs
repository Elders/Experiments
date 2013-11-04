using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Protoreg;
using RabbitMQ.Client;

namespace Store
{

    internal class RabbitMqStore<T> : IDisposable where T : class
    {
        private IModel errorChannel;
        private string errorQueueName;
        public const string ProjectionsQueueName = "Hyperion-MarketVisionProjections";
        private static IConnection connection = new ConnectionFactory().CreateConnection();
        private QueueDeclareOk queue;
        private readonly string queueName;
        private Dictionary<T, BasicGetResult> results = new Dictionary<T, BasicGetResult>();
        private IModel channel;
        private readonly ProtoregSerializer serializer;

        public RabbitMqStore(string name, ProtoregSerializer serializer)
        {
            this.serializer = serializer;
            this.queueName = name;
            channel = connection.CreateModel();
            this.errorQueueName = name + "_error";
            channel = connection.CreateModel();
            queue = channel.QueueDeclare(queueName, true, false, false, null);
            errorChannel = connection.CreateModel();
            errorChannel.QueueDeclare(errorQueueName, true, false, false, null);
        }

        public void Acknowlidge(T item)
        {
            if (!results.ContainsKey(item))
                return;
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
            if (result == null || result.Body == null || result.Body.Length == 0)
                return null;
            var str = new MemoryStream(result.Body);
            str.Position = 0;
            T item;
            try
            {
                object res = serializer.Deserialize(str);
                item = (T)res;
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("Failed to deserialize IEvent from Persistenqueue {0}", ProjectionsQueueName);

                errorChannel.BasicPublish(String.Empty, errorQueueName, null, result.Body);
                channel.BasicAck(result.DeliveryTag, false);
                return null;
            }
            results.Add(item, result);
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
