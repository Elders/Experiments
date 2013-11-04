using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AsyncWork
{
    [DataContract(Name = "47ad43f8-b056-49ff-8f9b-913362f8667b")]
    public class PendingWork
    {
        PendingWork() { }

        public PendingWork(Guid workTypeId, Guid workId, object state, DateTime scheduleDate)
        {
            WorkTypeId = workTypeId;
            WorkId = workId;
            State = state;
            ScheduleDate = scheduleDate;
        }

        [DataMember(Order = 1)]
        public object State { get; set; }

        [DataMember(Order = 2)]
        public DateTime ScheduleDate { get; set; }

        [DataMember(Order = 3)]
        public Guid WorkId { get; set; }

        [DataMember(Order = 4)]
        public Guid WorkTypeId { get; private set; }
    }
}