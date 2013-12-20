using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Multithreading.Work;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;


namespace RabbitMQTest
{
    class Program
    {
        public static string QueueNameOne = "SpeedTextQueue";
        public static string ExchangeName = "SpeedTestExchange";
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "192.168.16.53",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            var pubConnection = factory.CreateConnection();
            var consumeConnection = factory.CreateConnection();
            using (var channel = pubConnection.CreateModel())
            {
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Headers, false, false, null);
                //Dictionary<string, object> headers = null;
                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers["asd"] = "asd";
                channel.QueueDeclare(QueueNameOne, true, false, false, headers);
                channel.QueueBind(QueueNameOne, ExchangeName, String.Empty, headers);
                //channel.QueueDeclare(QueueNameTwo, true, false, false, null);
                //channel.QueueBind(QueueNameTwo, ExchangeName, String.Empty);
            }
            var pool = new WorkPool("MyPoool", 5);
            for (int i = 0; i < 5; i++)
            {
                pool.AddWork(new Consumer(factory.CreateConnection()));
            }
            pool.StartCrawlers();
            //for (int i = 0; i < 5; i++)
            //{
            //    Task.Factory.StartNew(() =>
            //    {
            //        using (var channel = consumeConnection.CreateModel())
            //        {

            //            var consumer = new QueueingBasicConsumer(channel);
            //            channel.BasicConsume(QueueNameOne, true, consumer);
            //            while (true)
            //            {
            //                consumer.Queue.Dequeue();
            //            }
            //        }
            //    });

            //}
            for (int i = 0; i < 2; i++)
            {
                var message = BuildBytes(200);
                Task.Factory.StartNew(() =>
                {
                    using (var channel = pubConnection.CreateModel())
                    {
                        IBasicProperties props = new BasicProperties();
                        props.SetPersistent(true);
                        props.Priority = 9;
                        props.Headers = new Dictionary<string, object>() { { "asd", "asd" } };
                        while (true)
                        {
                            for (int j = 0; j < 100; j++)
                            {
                                channel.BasicPublish(ExchangeName, string.Empty, false, false, props, message);
                            }
                        }
                    }
                });
            }

        }

        static byte[] BuildBytes(int numberofByets)
        {
            var bytes = new byte[numberofByets];
            for (int i = 0; i < numberofByets; i++)
            {
                bytes[i] = 32;
            }
            return bytes;
        }

    }
    public class Consumer : IWork
    {

        public DateTime ScheduledStart { get; private set; }
        public IConnection ConsumeConnection { get; private set; }
        public Consumer(IConnection consumeConnection)
        {
            ConsumeConnection = consumeConnection;
        }
        public void Start()
        {
            using (var channel = ConsumeConnection.CreateModel())
            {

                var consumer = new QueueingBasicConsumer();
                channel.BasicConsume(Program.QueueNameOne, false, consumer);
                while (true)
                {
                    var msg = consumer.Queue.DequeueNoWait(null);
                    if (msg != null)
                        channel.BasicAck(msg.DeliveryTag, false);
                }
            }
        }
    }

    public static class MeasureExecutionTime
    {
        public static string Start(Action action)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action<int> action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action(i);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }
    }


}
