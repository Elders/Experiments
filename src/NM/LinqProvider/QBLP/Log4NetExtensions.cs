using System;
using System.Collections.Concurrent;
using System.Net;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public static class Log4NetExtensions
    {
        static ConcurrentDictionary<string, DateTime> deadList = new ConcurrentDictionary<string, DateTime>();

        public static void LogWebException(this log4net.ILog log, string server, string message, WebException ex, double timeoutThreshholdInMinutes = 60)
        {
            var now = DateTime.UtcNow;
            if (ex.Status == WebExceptionStatus.Timeout)
            {
                DateTime timeoutSince;
                if (!deadList.TryGetValue(server, out timeoutSince))
                {
                    timeoutSince = now;
                    deadList.AddOrUpdate(server, now, (key, val) => timeoutSince);
                }
                else if (timeoutSince.AddMinutes(timeoutThreshholdInMinutes) <= now)
                {
                    log.Error("Frequent timeouts detected for '" + server + "' since " + timeoutSince, ex);
                    deadList.TryRemove(server, out timeoutSince);
                }
            }
        }
    }
}
