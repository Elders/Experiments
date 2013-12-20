using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Heartbeat
{
    class Program
    {
        //192.168.16.46
        static void Main(string[] args)
        {
            var myIp = GetMyIp();
            //Heartbeat heartBeat = new Heartbeat();
            //heartBeat.Configure(cfg =>
            //    {
            //        cfg.SetCurrrentLocalRabbitMQEndpoint("guest", "guest", myIp, "Heartbeat");
            //        cfg.WithHeartbeat("guest", "guest", "192.168.16.46", "Backup");
            //        cfg.SetHeartbeatInterval(new TimeSpan(0, 0, 5));
            //        cfg.SetHeartbeatTolerance(new TimeSpan(0, 0, 10));
            //    });
            //heartBeat.OnDead += () => Console.WriteLine("Dead");
            //heartBeat.OnResurrect += () => Console.WriteLine("Resurected");
            //heartBeat.Start();
            //Console.ReadLine();

            IStatelessHeartbeat statelessHeartbeat = new StatelessRabbitMQHeartbeat(cfg =>
                {
                    cfg.SetPulseTimeoutTolerance(new TimeSpan(0, 0, 10));
                    cfg.SetCurrrentLocalRabbitMQEndpoint("guest", "guest", myIp, "StatelessPulse");
                });

            Thread.Sleep(3000);
            Console.WriteLine("SendPulse on enter");

            while (true)
            {
                Console.ReadLine();
                var result = statelessHeartbeat.Pulse(StatelessHeartbeatConfiguration.BuildUri("guest", "guest", "192.168.16.69", "StatelessPulse2"));
                if (result)
                    Console.WriteLine("Alive");
                else
                    Console.WriteLine("Dead");
            }


        }

        public static string GetMyIp()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
        
    }
}
