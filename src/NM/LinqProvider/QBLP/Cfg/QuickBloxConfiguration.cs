using System;
using System.Collections.Generic;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class QuickBloxConfiguration
    {
        IQuickBloxConfiguration configuration;

        readonly IDictionary<Type, string> eventToCustomObjectMappings;

        /// <summary>
        /// Initialises a <see cref="QuickBloxConfiguration"/> instance.
        /// </summary>
        /// <param name="quickBloxConfiguration">Configuration information for connecting to QuickBlox and information about custom object endpoints.</param>
        /// <param name="eventToCustomObjectMappings">A key-value collection for the relation between QuickBlox custom objects and Hyperion events. <see cref="HyperionConfiguration"/> may have such collection.</param>
        public QuickBloxConfiguration(IQuickBloxConfiguration quickBloxConfiguration, IDictionary<Type, string> eventToCustomObjectMappings)
        {
            this.eventToCustomObjectMappings = eventToCustomObjectMappings;
            configuration = quickBloxConfiguration;
        }

        /// <summary>
        /// Creates a new <see cref="QuickBloxSessionFactory"/> instance.
        /// </summary>
        /// <returns>Returns a new <see cref="QuickBloxSessionFactory"/> instance.</returns>
        public QuickBloxSessionFactory BuildSessionFactory()
        {
            return new QuickBloxSessionFactory(configuration, eventToCustomObjectMappings);
        }
    }
}