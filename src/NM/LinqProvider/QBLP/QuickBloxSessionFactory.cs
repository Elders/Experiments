using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using IQToolkit;
using LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg;
using LinqProvider.LP;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    /// <summary>
    /// Holds all information needed for creating a new <see cref="QuickBloxSession"/>.
    /// </summary>
    public class QuickBloxSessionFactory
    {
        public QueryProvider Provider;

        IQuickBloxConfiguration configuration;

        SessionWrap currentSession;

        private readonly IDictionary<Type, string> eventToCustomObjectMappings;

        object locker = new object();

        internal QuickBloxSessionFactory(IQuickBloxConfiguration configuration, IDictionary<Type, string> eventToCustomObjectMappings)
        {
            this.eventToCustomObjectMappings = eventToCustomObjectMappings;
            eventIdToEventTypeMappings = eventToCustomObjectMappings.ToDictionary(x => new Guid((x.Key.GetCustomAttributes(false).Where(y => y.GetType() == typeof(DataContractAttribute)).FirstOrDefault() as DataContractAttribute).Name), z => z.Key);
            this.configuration = configuration;
        }

        public IDictionary<Guid, Type> EventIdToEventTypeMappings { get { return eventIdToEventTypeMappings; } }

        public IDictionary<Type, string> EventToCustomObjectMappings { get { return eventToCustomObjectMappings; } }

        public QuickBloxSession GetCurrentSession()
        {
            if (currentSession == null || currentSession.Expiration <= DateTime.UtcNow)
            {
                OpenSession();
            }
            return currentSession.Session;
        }

        /// <summary>
        /// Opens a new <see cref="QuickBloxSession"/>.
        /// </summary>
        /// <returns>Returns QuickBlox session object for communicating with the QuickBlox' REST Api.</returns>
        public QuickBloxSession OpenSession()
        {
            lock (locker)
            {
                if (currentSession == null || currentSession.Expiration <= DateTime.UtcNow)
                {
                    var session = String.IsNullOrWhiteSpace(configuration.Username) || String.IsNullOrWhiteSpace(configuration.UserPassword)
                        ? OpenAnonymousSession()
                        : OpenUserSession();
                    session.SessionFactory = this;
                    Provider = new QuickBloxQueryProvider(configuration.Endpoint, session.Token);
                    currentSession = new SessionWrap(session);
                }
            }
            return currentSession.Session;
        }

        QuickBloxSession OpenAnonymousSession()
        {
            var response = QuickBloxApiClient.RequestNewAnonymousSession(configuration.SessionUrl, configuration.ApplicationId, configuration.AuthorizationKey, configuration.AuthorizationSecret);
            var session = ParseSessionCreationResponse(response.GetResponseStream());
            response.Close();
            return session;
        }

        QuickBloxSession OpenUserSession()
        {
            var response = QuickBloxApiClient.RequestNewUserSession(configuration.SessionUrl, configuration.ApplicationId, configuration.AuthorizationKey, configuration.AuthorizationSecret, configuration.Username, configuration.UserPassword);
            var session = ParseSessionCreationResponse(response.GetResponseStream());
            response.Close();
            return session;
        }

        QuickBloxSession ParseSessionCreationResponse(Stream sessionCreationResponse)
        {
            XDocument xmlDoc = XDocument.Load(sessionCreationResponse);

            return (from tutorial in xmlDoc.Descendants("session")
                    select new QuickBloxSession(tutorial.Element("token").Value))
                    .SingleOrDefault();

        }

        IDictionary<Guid, Type> eventIdToEventTypeMappings;

        class SessionWrap
        {
            public SessionWrap(QuickBloxSession session)
            {
                Session = session;
                Expiration = DateTime.UtcNow.AddHours(1).AddMinutes(30);
            }
            public QuickBloxSession Session { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}