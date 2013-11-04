using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protoreg;
using Store;

namespace AsyncWork
{
    public class RabbitMqManagerStateRepository : IWorkManagerStateRepository
    {
        RabbitMqStore<ManagerState> RabbitMqStore;

        public RabbitMqManagerStateRepository(ProtoregSerializer serializer)
        {
            RabbitMqStore = new RabbitMqStore<ManagerState>("AsyncWork-StateRepository", serializer);
        }

        ManagerState loaded;

        public ManagerState Load()
        {
            List<ManagerState> states = new List<ManagerState>();
            while (true)
            {
                var other = RabbitMqStore.Dequeue();
                if (other == null)
                    break;
                states.Add(other);
            }
            var latest = states.OrderByDescending(x => x.Revision).FirstOrDefault();
            if (latest == null)
                return latest;
            RabbitMqStore.AcknowlidgeRange(states.Where(x => x.Revision != latest.Revision));
            loaded = latest;
            return latest;
        }

        public void SaveOrUpdate(ref ManagerState state)
        {
            if (loaded == null)
            {
                state.Revision = state.Revision + 1;
                RabbitMqStore.Enqueue(state);
                state = Load();
            }
            else if (loaded != state)
            {
                throw new ArgumentException("The current state was not loaded from this repository.This repository can contain only one item ^^");
            }
            else
            {
                state.Revision = state.Revision + 1;
                RabbitMqStore.Enqueue(state);
                RabbitMqStore.Acknowlidge(state);
                state = Load();
            }
        }
    }
}
