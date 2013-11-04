using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Serialization;
using Protoreg;

namespace AsyncWork.AsynkWorkWithRabbitMq
{
    public class WorkProcessorsConfigurator
    {
        private readonly int conccurentNumberOfProcessors;
        private IMessageSerializer massTransitSerializer;
        private WorkProcessor processor;
        static IServiceBus serviceBus;
        private readonly IProcessState workProcessor;

        public WorkProcessorsConfigurator(IProcessState workProcessor, ProtoregSerializer protoregSerializer, int conccurentNumberOfProcessors = 0)
        {
            this.workProcessor = workProcessor;
            this.conccurentNumberOfProcessors = conccurentNumberOfProcessors;
            massTransitSerializer = new MassTransitSerializer(protoregSerializer);
        }

        public void Start()
        {
            processor = new WorkProcessor(workProcessor);
            serviceBus = ServiceBusFactory.New(sbc =>
            {
                sbc.ReceiveFrom("rabbitmq://localhost/AsyncWork-PendingWork");
                sbc.UseRabbitMq();
                sbc.SetPurgeOnStartup(false);
                sbc.SetDefaultSerializer(massTransitSerializer);
                if (conccurentNumberOfProcessors > 0)
                    sbc.SetConcurrentConsumerLimit(conccurentNumberOfProcessors);
                sbc.Subscribe(subs =>
                {
                    subs.Consumer(typeof(WorkProcessor), x => processor);
                });
            });
        }

        public void Stop()
        {
            serviceBus = null;
        }


    }
}
