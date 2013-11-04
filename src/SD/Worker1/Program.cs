using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncWork;
using AsyncWork.AsynkWorkWithRabbitMq;
using MassTransit;
using Protoreg;

namespace Worker1
{
    class Program
    {
        public static IServiceBus serviceBus { get; set; }
        static void Main(string[] args)
        {
            ProtoRegistration registration = new ProtoRegistration();
            registration.RegisterAssembly<ManagerState>();
            registration.RegisterCommonType<Work>(); // Register contract
            ProtoregSerializer protoregSerializer = new ProtoregSerializer(registration);
            protoregSerializer.Build();

            var processor = new WorkProcessorsConfigurator(new MyWorkProcessor(), protoregSerializer);
            processor.Start();
            Console.ReadLine();
        }

        public class MyWorkProcessor : IProcessState
        {
            public void DoWork(Work state)
            {
                Console.WriteLine("Processing work {0}", state.SkipValue);
            }

            public void ProcessWork(object state)
            {
                DoWork((dynamic)state);
            }
        }

    }
}
