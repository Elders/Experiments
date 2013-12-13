using System;
using System.Configuration;
using System.IO;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class PoolCollection : ConfigurationElementCollection
    {
        public PoolSettings this[int index]
        {
            get { return (PoolSettings)BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PoolSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PoolSettings)element).Name; ;
        }

    }
}