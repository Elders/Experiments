using System.Collections.Generic;
namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public interface IQuickBloxConfiguration
    {
        /// <summary>
        /// Application identifier. This identifier is created at the time of adding a new application to your QuickBlox Account through the web interface. 
        /// You can not set it yourself. You should use this identifier in your API Application to get access to QuickBlox through the API interface.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// API Application identification key. This key is created at the time of adding a new application to your QuickBlox Account through the web interface. 
        /// You can not set it yourself. You should use this key in your API Application to get access to QuickBlox through the API interface.
        /// </summary>
        string AuthorizationKey { get; }

        /// <summary>
        /// Secret sequence which is used to prove Authentication Key. It's similar to a password. You have to keep it private and restrict access to it. 
        /// Use it in your API Application to create your signature for authentication request.
        /// </summary>
        string AuthorizationSecret { get; }

        /// <summary>
        /// The URL where the application is hosted.
        /// </summary>
        string Endpoint { get; }

        /// <summary>
        /// The URL where new session can be requested.
        /// </summary>
        string SessionUrl { get; }

        /// <summary>
        /// API user login.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// API user password.
        /// </summary>
        string UserPassword { get; }
    }
}