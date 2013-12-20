using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Heartbeat;
using Heartbeat1;

namespace MasstransitQueues
{
    class Program
    {//192.168.16.46
        static void Main(string[] args)
        {
            var myIp = GetMyIp();
            //Heartbeat.Heartbeat heartBeat = new Heartbeat.Heartbeat();
            //heartBeat.Configure(cfg =>
            //{
            //    cfg.SetCurrrentLocalRabbitMQEndpoint("guest", "guest", myIp, "Backup");
            //    //   cfg.SetRemoteHeartbeatEndpoint("guest", "guest", myIp, "Heartbeat");
            //    cfg.SetHeartbeatInterval(new TimeSpan(0, 0, 5));
            //    cfg.SetHeartbeatTolerance(new TimeSpan(0, 0, 10));

            //});
            //heartBeat.OnDead += () => Console.WriteLine("Dead");
            //heartBeat.OnResurrect += () => Console.WriteLine("Resurected");
            //heartBeat.Start();
            IStatelessHeartbeat statelessHeartbeat2 = new StatelessRabbitMQHeartbeat(cfg =>
            {
                cfg.SetPulseTimeoutTolerance(new TimeSpan(0, 0, 10));
                cfg.SetCurrrentLocalRabbitMQEndpoint("guest", "guest", myIp, "StatelessPulse2");
            });

            Console.ReadLine();

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
