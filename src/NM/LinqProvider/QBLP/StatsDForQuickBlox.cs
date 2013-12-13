using System;
using StatsdClient;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public static class StatsDForQuickBlox
    {
        public static TimingToken LogQuickBloxTiming(this Statsd statsd, string name)
        {
            string stat = String.Format("{0}.LaCore.Hyperion.QuickBlox.{1}", Environment.MachineName, name);
            return statsd.LogTiming(stat);
        }

        public static void LogQuickBloxCount(this Statsd statsd, string name, int count = 1)
        {
            string stat = String.Format("{0}.LaCore.Hyperion.QuickBlox.{1}", Environment.MachineName, name);
            statsd.LogCount(stat, count);
        }
    }
}
