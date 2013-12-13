using System;
using System.Configuration;
using System.IO;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class PoolSettings : ConfigurationElement
    {
        [ConfigurationProperty("events")]
        [ConfigurationCollection(typeof(EventCollection), AddItemName = "event")]
        public EventCollection Events
        {
            get { return (EventCollection)this["events"]; }
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("workers")]
        public int Workers
        {
            get { return (int)this["workers"]; }
        }
    }
}