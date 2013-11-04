using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using AsyncWork.AsynkWorkWithRabbitMq;
using MassTransit;

namespace AsyncWork
{
    public class WorkProcessor : Consumes<PendingWork>.Context
    {
        private readonly IProcessState stateProcessor;

        public WorkProcessor(IProcessState stateProcessor)
        {
            this.stateProcessor = stateProcessor;
        }

        public void Consume(IConsumeContext<PendingWork> context)
        {
            stateProcessor.ProcessWork(context.Message.State);
            context.Bus.Publish(new AsyncWorkDone(context.Message.WorkTypeId, context.Message.WorkId, context.Message.State, DateTime.UtcNow));
        }

    }
}