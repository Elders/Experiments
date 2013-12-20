using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Heartbeat.Messages;
using MassTransit;

namespace Heartbeat
{
    public class Heartbeat : Consumes<HeartbeatAcknowliged>.Context,
        Consumes<HeartbeatImAlive>.Context
    {
        HeartbeatConfiguration cfg;

        public Timer HeartbeatTimer;

        bool isMasterDead;

        HeartbeatAcknowliged lastAcknowlige;

        private HeartbeatImAlive lastHeartbeat;

        private Timer monitorTimer;

        IServiceBus ServiceBus;

        public event Action OnDead;

        public event Action OnResurrect;

        public Heartbeat(Action<HeartbeatConfiguration> configureAction)
        {
            Configure(configureAction);
        }

        public Heartbeat() { }

        public void Configure(Action<HeartbeatConfiguration> configure)
        {
            cfg = new HeartbeatConfiguration();
            configure(cfg);
            if (!cfg.IsConfigured())
                throw new ArgumentException("Invalid or not full configuration");
        }

        public void Consume(IConsumeContext<HeartbeatAcknowliged> context)
        {
            Console.WriteLine("HeartbeatAcknowliged");
            lastAcknowlige = context.Message;
            if (isMasterDead)
                NotifyAlive();
        }

        public void Consume(IConsumeContext<HeartbeatImAlive> context)
        {
            Console.WriteLine("HeartbeatImAlive");
            lastHeartbeat = context.Message;
            context.Respond(new HeartbeatAcknowliged() { Timestamp = DateTime.UtcNow });
            //context.Bus.GetEndpoint(context.ResponseAddress)
            if (isMasterDead)
                NotifyAlive();
        }

        public void Start()
        {
            if (OnDead == null || OnResurrect == null || cfg == null || !cfg.IsConfigured())
            {
                throw new InvalidOperationException("You can not start the heartbeat without a prober configuration or not setting OnRessurect or OnDead events");
            }
            ServiceBus = ServiceBusFactory.New(x =>
             {
                 x.ReceiveFrom(cfg.CurrentLocalEndpoint);
                 x.UseRabbitMq();
                 x.Subscribe(sbc => sbc.Instance(this).Permanent());
             });
            lastAcknowlige = new HeartbeatAcknowliged() { Timestamp = DateTime.UtcNow };
            lastHeartbeat = new HeartbeatImAlive() { Timestamp = DateTime.UtcNow };
            if (cfg.SendPulse)
                HeartbeatTimer = new Timer(new TimerCallback(SendPulse), null, 0, (int)(cfg.HeartbeatInterval.TotalMilliseconds));
            monitorTimer = new Timer(Monitor, this, 1000, (int)(cfg.HeartbeatInterval.TotalMilliseconds));

        }

        public void Stop()
        {
            ServiceBus = null;
            HeartbeatTimer = null;
        }

        private bool CheckExternalForHeartbeat()
        {
            //HIT API
            return false;
        }

        private void Monitor(object state)
        {
            var check = DateTime.UtcNow;
            if (!isMasterDead)
            {

                if ((cfg.SendPulse) && (check - lastAcknowlige.Timestamp).TotalMilliseconds > cfg.HeartbeatTolerance.TotalMilliseconds)
                    NotifyDead();
                if ((!cfg.SendPulse) && (check - lastHeartbeat.Timestamp).TotalMilliseconds > cfg.HeartbeatTolerance.TotalMilliseconds)
                    NotifyDead();
            }
        }

        private void NotifyAlive()
        {
            isMasterDead = false;
            if (cfg.SendPulse)
                HeartbeatTimer.Change(1000, (int)(cfg.HeartbeatInterval.TotalMilliseconds));
            OnResurrect();
        }

        private void NotifyDead()
        {
            if (!CheckExternalForHeartbeat())
            {
                isMasterDead = true;
                if (cfg.SendPulse)
                    HeartbeatTimer.Change(1000, 1000);
                OnDead();
            }
        }

        private void SendPulse(object state)
        {
            try
            {
                Console.WriteLine("Pulse");
                ServiceBus.GetEndpoint(cfg.HeartbeatEndpoint).Send(new HeartbeatImAlive() { Timestamp = DateTime.UtcNow }, x => x.SetResponseAddress(cfg.CurrentLocalEndpoint));
            }
            catch (Exception ex)
            {
                if (!isMasterDead)
                {
                    NotifyDead();
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
