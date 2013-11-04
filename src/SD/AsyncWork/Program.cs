using System;
using System.Collections.Generic;
using System.Linq;
using AsyncWork.AsynkWorkWithRabbitMq;
using Protoreg;

namespace AsyncWork
{
    public class Program
    {

        static void Main(string[] args)
        {

            ProtoRegistration registration = new ProtoRegistration();
            registration.RegisterAssembly<ManagerState>();
            registration.RegisterCommonType<Work>(); // Register contract
            ProtoregSerializer protoregSerializer = new ProtoregSerializer(registration);
            protoregSerializer.Build();

            var pool = new WorkManagerConfigurator(new MyWorkManager(), protoregSerializer);
            for (int i = 0; i < 10; i++)
            {
                pool.AddWork(new Work(i));
            }

            pool.Start();

            Console.WriteLine("Console writeline");
            Console.ReadLine();
            pool.Stop();
        }
        public class MyWorkManager : IUpdateState
        {
            public object UpdateState(object state)
            {
                return ProcessWork((dynamic)state);
            }

            public Work ProcessWork(Work state)
            {
                state.SkipValue += 1;
                return state;
            }


        }
    }
}