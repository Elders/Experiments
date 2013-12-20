using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Heartbeat
{
    public class StatelessHeartbeatConfiguration
    {
        const string EndpointFormat = "rabbitmq://{0}:{1}@{2}/{3}";

        public Uri CurrentLocalEndpoint { get; private set; }

        public TimeSpan PulseTimeoutTolerance { get; private set; }

        public bool IsConfigured()
        {
            return !(CurrentLocalEndpoint == null || PulseTimeoutTolerance == default(TimeSpan));
        }

        public void SetCurrrentLocalRabbitMQEndpoint(string username, string password, string ip, string queueName)
        {
            CurrentLocalEndpoint = BuildUri(username, password, ip, queueName);
        }

        public void SetPulseTimeoutTolerance(TimeSpan heartbeatTolerance)
        {
            PulseTimeoutTolerance = heartbeatTolerance;
        }

        public static Uri BuildUri(string username, string password, string ip, string queueName)
        {
            return new Uri(String.Format(EndpointFormat, username, password, ip, queueName));
        }

    }
}
