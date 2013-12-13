using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public class QuickBloxSettings : ConfigurationElement, IQuickBloxConfiguration
    {
        [ConfigurationProperty("application-id")]
        public string ApplicationId
        {
            get { return (string)this["application-id"]; }
        }

        [ConfigurationProperty("authorization-key")]
        public string AuthorizationKey
        {
            get { return (string)this["authorization-key"]; }
        }

        [ConfigurationProperty("authorization-secret")]
        public string AuthorizationSecret
        {
            get { return (string)this["authorization-secret"]; }
        }

        [ConfigurationProperty("endpoint")]
        public string Endpoint
        {
            get { return ((string)this["endpoint"]).TrimEnd('/'); }
        }

        public string SessionUrl
        {
            get { return Endpoint + "/session.xml"; }
        }

        [ConfigurationProperty("username", DefaultValue = "")]
        public string Username
        {
            get { return (string)this["username"]; }
        }

        [ConfigurationProperty("userpassword", DefaultValue = "")]
        public string UserPassword
        {
            get { return (string)this["userpassword"]; }
        }
    }
}