using System;
using System.Configuration;
using System.IO;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class EventSettings : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("source")]
        public string Source
        {
            get { return (string)this["source"]; }
        }

        [ConfigurationProperty("max-delay", DefaultValue = "5000")]
        public int MaxDelay
        {
            get { return (int)this["max-delay"]; }
        }
    }
}