using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heartbeat
{
    public interface IStatelessHeartbeat
    {
        bool Pulse(Uri targetUrl);
    }
}
