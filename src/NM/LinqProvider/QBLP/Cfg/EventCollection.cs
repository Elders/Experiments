using System;
using System.Configuration;
using System.IO;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class EventCollection : ConfigurationElementCollection
    {
        public EventSettings this[int index]
        {
            get { return (EventSettings)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EventSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventSettings)element).Name;
        }

    }
}