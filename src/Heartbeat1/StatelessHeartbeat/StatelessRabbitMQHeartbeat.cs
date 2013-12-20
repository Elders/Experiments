using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Exceptions;

namespace Heartbeat
{
    public class StatelessRabbitMQHeartbeat : IStatelessHeartbeat//,
    // Consumes<SinglePulse>.Context
    {
        StatelessHeartbeatConfiguration cfg;

        IServiceBus bus;

        public StatelessRabbitMQHeartbeat(Action<StatelessHeartbeatConfiguration> configureAction)
        {
            Configure(configureAction);
        }

        public StatelessRabbitMQHeartbeat() { }

        public void Configure(Action<StatelessHeartbeatConfiguration> configure)
        {
            cfg = new StatelessHeartbeatConfiguration();
            configure(cfg);
            if (!cfg.IsConfigured())
                throw new ArgumentException("Invalid or not full configuration");

            bus = ServiceBusFactory.New(x =>
            {
                x.ReceiveFrom(cfg.CurrentLocalEndpoint);
                x.UseRabbitMq();
                x.Subscribe(sbc => sbc.Instance(this));

            });

        }

        public bool Pulse(Uri targetUrl)
        {

            var response = false;
            IServiceBus RemoteBus = null;
            try
            {
                RemoteBus = ServiceBusFactory.New(x =>
                 {
                     x.ReceiveFrom(targetUrl);
                     x.UseRabbitMq();
                     x.Subscribe(sbc =>
                     {
                         sbc.Handler<SinglePulse>((y, z) =>
                             {
                                 y.Respond(new SinglePulseResponse() { Timestamp = DateTime.UtcNow });
                             });


                     });
                 });

                RemoteBus.GetEndpoint(targetUrl).SendRequest(new SinglePulse() { Timestamp = DateTime.UtcNow }, bus, requestCfg =>
                      {
                          var asd = requestCfg.RequestId;
                          requestCfg.SetTimeout(cfg.PulseTimeoutTolerance);
                          requestCfg.Handle<SinglePulseResponse>(x =>
                          {
                              response = true;
                          });

                      });
                
            }
            catch (RequestTimeoutException ex)
            {
                response = false;
            }
            catch (Exception ex)
            {
                //log
                response = false;
            }
            finally
            {
                if (RemoteBus != null)
                    Task.Run(() => RemoteBus.Dispose());

            }
            return response;

            
        }

        //public void Consume(IConsumeContext<SinglePulse> context)
        //{
        //    context.Respond(new SinglePulseResponse() { Timestamp = DateTime.UtcNow });
        //    int breakpoint = 0;
        //    breakpoint = 3;
        //    Console.WriteLine("Pulse recieved");
        //}
    }
    public class SinglePulse
    {
        public DateTime Timestamp { get; set; }
        
    }
    public class SinglePulseResponse
    {
        public DateTime Timestamp { get; set; }

    }
}
