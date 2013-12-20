using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Heartbeat1
{
    public class Service
    {
        private string serviceName;
        private double timeoutMilliseconds = 20;

        public Service(string serviceName)
        {
            this.serviceName = serviceName;
        }
        public Service(string serviceName, double timeoutMilliseconds)
        {
            this.serviceName = serviceName;
            this.timeoutMilliseconds = timeoutMilliseconds;
        }

        public void StartService()
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public void StopService()
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }
    }
}
