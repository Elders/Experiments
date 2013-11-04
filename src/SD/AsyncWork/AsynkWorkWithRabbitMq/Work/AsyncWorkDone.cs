using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AsyncWork
{
    public interface IState { }

    [DataContract(Name = "191e496d-9fa5-41a1-a6ec-f1ecde8fc2e8")]
    public class AsyncWorkDone
    {
        AsyncWorkDone() { }

        public AsyncWorkDone(Guid workTypeId, Guid workId, object state, DateTime dateDone)
        {
            WorkTypeId = workTypeId;
            NextWorkId = Guid.NewGuid();
            WorkId = workId;
            State = state;
            DateDone = dateDone;
        }

        [DataMember(Order = 1)]
        public object State { get; set; }

        [DataMember(Order = 2)]
        public DateTime DateDone { get; set; }

        [DataMember(Order = 3)]
        public Guid WorkId { get; set; }

        [DataMember(Order = 4)]
        public Guid NextWorkId { get; set; }

        [DataMember(Order = 5)]
        public Guid WorkTypeId { get; private set; }
    }
}