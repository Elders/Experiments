using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using MassTransit.Serialization;
using Protoreg;

namespace AsyncWork.AsynkWorkWithRabbitMq
{
    public class WorkManagerConfigurator
    {
        static WorkManager mngr;
        static IServiceBus serviceBusManager;
        public List<object> workItems = new List<object>();

        public WorkManagerConfigurator(IUpdateState workManager, ProtoregSerializer protoregSerializer)
        {
            IMessageSerializer massTransitSerializer = new MassTransitSerializer(protoregSerializer);

            mngr = new WorkManager(workManager, new RabbitMqManagerStateRepository(protoregSerializer));
            serviceBusManager = ServiceBusFactory.New(sbc =>
            {
                sbc.ReceiveFrom("rabbitmq://localhost/AsyncWork-WorkDone");
                sbc.UseRabbitMq();
                sbc.SetPurgeOnStartup(false);
                sbc.SetDefaultSerializer(massTransitSerializer);
                sbc.SetConcurrentConsumerLimit(1);
                sbc.Subscribe(subs =>
                {
                    subs.Instance(mngr);

                });
            });
        }

        public void AddWork(object work)
        {
            if (serviceBusManager != null)
                workItems.Add(work);
        }

        public void Start()
        {
            if (serviceBusManager != null)
            {
                foreach (var item in workItems)
                {
                    mngr.DistributeWork(serviceBusManager, new PendingWork(Guid.NewGuid(), Guid.NewGuid(), item, DateTime.UtcNow));
                }
            }
        }

        public void Stop()
        {
            serviceBusManager = null;
        }
    }
}