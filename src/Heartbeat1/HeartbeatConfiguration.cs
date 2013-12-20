using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Heartbeat
{
    public class HeartbeatConfiguration
    {
        public Uri HeartbeatEndpoint { get; private set; }
        public Uri CurrentLocalEndpoint { get; private set; }
        public TimeSpan HeartbeatInterval { get; private set; }
        public TimeSpan HeartbeatTolerance { get; private set; }
        public bool SendPulse { get; private set; }

        const string EndpointFormat = "rabbitmq://{0}:{1}@{2}/{3}";

        public void SetCurrrentLocalRabbitMQEndpoint(string username, string password, string ip, string queueName)
        {
            CurrentLocalEndpoint = BuildUri(username, password, ip, queueName);
        }

        public void WithHeartbeat(string username, string password, string ip, string queueName)
        {
            SendPulse = true;
            HeartbeatEndpoint = BuildUri(username, password, ip, queueName);
        }


        private Uri BuildUri(string username, string password, string ip, string queueName)
        {
            return new Uri(String.Format(EndpointFormat, username, password, ip, queueName));
        }

        public bool IsConfigured()
        {
            return !((HeartbeatEndpoint == null && SendPulse == true) || CurrentLocalEndpoint == null || HeartbeatInterval == default(TimeSpan) || HeartbeatTolerance == default(TimeSpan));
        }

        public void SetHeartbeatInterval(TimeSpan heartbeatInterval)
        {
            HeartbeatInterval = heartbeatInterval;
        }

        public void SetHeartbeatTolerance(TimeSpan heartbeatTolerance)
        {
            HeartbeatTolerance = heartbeatTolerance;
        }
    }
}
