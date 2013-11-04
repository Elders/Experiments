using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AsyncWork
{
    [DataContract(Name = "8ff3f9e9-a0b8-4d82-bb46-c9a66b31e087")]
    public class ManagerState
    {
        public ManagerState()
        {
            ProcessedResults = new Dictionary<Guid, Guid>();
        }

        public ManagerState(Dictionary<Guid, Guid> processedResults, int revision)
        {
            ProcessedResults = processedResults;
            Revision = revision;
        }
        [DataMember(Order = 1)]
        public Dictionary<Guid, Guid> ProcessedResults { get; set; }

        [DataMember(Order = 2)]
        public int Revision { get; set; }
    }
}