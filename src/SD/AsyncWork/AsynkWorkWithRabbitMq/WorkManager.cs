using System;
using System.Collections.Generic;
using System.Linq;
using AsyncWork.AsynkWorkWithRabbitMq;
using MassTransit;

namespace AsyncWork
{
    public class WorkManager : Consumes<AsyncWorkDone>.Context
    {
        static object Locker = new Object();

        private readonly IWorkManagerStateRepository repo;

        static ManagerState managerState;

        private readonly IUpdateState stateUpdater;

        public WorkManager(IUpdateState stateUpdater, IWorkManagerStateRepository repo)
        {
            this.stateUpdater = stateUpdater;
            this.repo = repo;
            managerState = repo.Load();   //  Load From Database
            if (managerState == null)
            {
                managerState = new ManagerState(new Dictionary<Guid, Guid>(), 0);
                repo.SaveOrUpdate(ref managerState);
            }
        }

        public void DistributeWork(IServiceBus bus, PendingWork work)
        {

            SaveManagerState(work);
            bus.Publish(work);
        }

        public void Consume(IConsumeContext<AsyncWorkDone> context)
        {
            if (Accept(context.Message))
            {
                var updatedState = ((dynamic)stateUpdater).UpdateState((dynamic)(context.Message.State));
                var work = new PendingWork(context.Message.WorkTypeId, context.Message.NextWorkId, updatedState, DateTime.UtcNow);
                context.Bus.Publish(work);
                SaveManagerState(work);
            }
        }

        private void SaveManagerState(PendingWork work)
        {
            lock (Locker)
            {
                managerState.ProcessedResults[work.WorkTypeId] = work.WorkId;
                repo.SaveOrUpdate(ref managerState);
            }
        }

        public bool Accept(AsyncWorkDone work)
        {
            bool accept;
            lock (Locker)
            {
                accept = managerState.ProcessedResults[work.WorkTypeId] == work.WorkId;
            }
            return accept;
        }
    }
}