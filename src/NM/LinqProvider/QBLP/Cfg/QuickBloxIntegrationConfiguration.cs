using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class QuickBloxIntegrationConfiguration : ConfigurationSection
    {
        static IDictionary<Type, string> eventToCustomObjectMapping = new Dictionary<Type, string>();

        static QuickBloxIntegrationConfiguration instance = null;

        public IDictionary<Type, string> EventToCustomObjectMappings { get { return eventToCustomObjectMapping; } }

        [ConfigurationProperty("pools")]
        [ConfigurationCollection(typeof(PoolCollection), AddItemName = "pool")]
        public PoolCollection Pools
        {
            get { return (PoolCollection)this["pools"]; }
        }

        [ConfigurationProperty("quickblox")]
        public QuickBloxSettings QuickBlox
        {
            get { return (QuickBloxSettings)this["quickblox"]; }
        }

        ///<summary>
        ///Get this configuration set from the application's default config file
        ///</summary>
        public static QuickBloxIntegrationConfiguration Load(params Assembly[] assemblyContainingEvents)
        {
            QuickBloxIntegrationConfiguration hyperionConfiguration;

            System.Reflection.Assembly assy = System.Reflection.Assembly.GetEntryAssembly();
            string assemblyDir = new FileInfo(assy.Location).DirectoryName;

            var appConfig = (assy.GetName().CodeBase + ".config").Replace("file:///", String.Empty);
            string hyperionConfig = Path.Combine(assemblyDir, @"quickbloxintegration.config.nmx");

            if (TryLoadConfig(appConfig, out hyperionConfiguration, assemblyContainingEvents))
                return hyperionConfiguration;
            else if (TryLoadConfig(hyperionConfig, out hyperionConfiguration, assemblyContainingEvents))
                return hyperionConfiguration;
            else
                throw new ConfigurationErrorsException("Cannot find any Hyperion configuration. Please add 'Hyperion' section in 'App.config' or create 'quickbloxintegration.config.nmx'");
        }

        ///<summary>
        /// Get this configuration set from a specific config file
        ///</summary>
        public static QuickBloxIntegrationConfiguration Load(string path, params Assembly[] assemblyContainingEvents)
        {
            if ((object)instance == null)
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap() { ExeConfigFilename = path };
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                if (config.Sections["quickbloxintegration"] == null)
                    return null;
                else
                {
                    instance = (QuickBloxIntegrationConfiguration)config.Sections["quickbloxintegration"];
                    BuildMappings(assemblyContainingEvents);
                }
            }
            return instance;
        }

        static void BuildMappings(params Assembly[] assemblyContainingEvents)
        {
            var eventTypes = assemblyContainingEvents.ToList().SelectMany(x => x.GetTypes()).ToList();
            foreach (PoolSettings pool in instance.Pools)
            {
                foreach (EventSettings evnt in pool.Events)
                {
                    var eventType = eventTypes.Where(e => e.Name == evnt.Name).FirstOrDefault();
                    if (eventType != null)
                        eventToCustomObjectMapping.Add(eventType, evnt.Source);
                    else
                    {
                        //  TODO:   Log warning that event type is not found.
                    }
                }
            }
        }

        static bool TryLoadConfig(string config, out QuickBloxIntegrationConfiguration hyperionConfiguration, params Assembly[] assemblyContainingEvents)
        {
            hyperionConfiguration = null;

            if (File.Exists(config))
                hyperionConfiguration = Load(config, assemblyContainingEvents);

            return hyperionConfiguration != null;
        }

    }
}