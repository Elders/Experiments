using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Serialization;
using Protoreg;

namespace AsyncWork
{
    [DataContract(Name = "e147a9ac-4d2c-453a-b0c1-b301ebef3b92")]
    public class Work
    {
        public Work()
        {

        }

        public Work(int skipValue)
        {
            SkipValue = skipValue;
        }
        [DataMember(Order = 1)]
        public int SkipValue { get; set; }
    }
}
