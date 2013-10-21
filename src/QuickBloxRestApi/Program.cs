using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuickBloxRestApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var sessionFactory = new QuickBloxSessionFactoryBuilder()
                .ApplicationId("3742")
                .AuthorizationKey("zdkwAdrd7xC5ZSP")
                .AuthorizationSecret("YbCkSctD-Y4M2dX")
                .BuildSessionFactory();

            var session = sessionFactory.OpenSession();
        }
    }

    public class QuickBloxSessionFactoryBuilder
    {
        Uri quickBloxApiUri = new Uri("https://api.quickblox.com");
        Uri quickBloxApiSessionUri = new Uri("https://api.quickblox.com/session.xml");
        string applicationId;
        string authorizationKey;
        string authorizationSecret;

        /// <summary>
        /// The location of QuickBlox REST Api. This is a set of HTTP/HTTPS calls and responses in XML or JSON formats. 
        /// </summary>
        /// <param name="quickBloxApiUri">The location of the QuickBlox REST api. Default is: https://api.quickblox.com.</param>
        public QuickBloxSessionFactoryBuilder ApiLocation(Uri quickBloxApiUri)
        {
            this.quickBloxApiUri = quickBloxApiUri;
            return this;
        }

        /// <summary>
        /// Application identifier. This identifier is created at the time of adding a new application to your QuickBlox Account through the web interface. 
        /// You can not set it yourself. You should use this identifier in your API Application to get access to QuickBlox through the API interface.
        /// </summary>
        /// <param name="applicationId">The application identifier for the current application.</param>
        public QuickBloxSessionFactoryBuilder ApplicationId(string applicationId)
        {
            this.applicationId = applicationId;
            return this;
        }

        /// <summary>
        /// API Application identification key. This key is created at the time of adding a new application to your QuickBlox Account through the web interface. 
        /// You can not set it yourself. You should use this key in your API Application to get access to QuickBlox through the API interface.
        /// </summary>
        /// <param name="authorizationKey">The authorization key for the current application.</param>
        public QuickBloxSessionFactoryBuilder AuthorizationKey(string authorizationKey)
        {
            this.authorizationKey = authorizationKey;
            return this;
        }

        /// <summary>
        /// Secret sequence which is used to prove Authentication Key. It's similar to a password. You have to keep it private and restrict access to it. 
        /// Use it in your API Application to create your signature for authentication request.
        /// </summary>
        /// <param name="authorizationSecret">The authorization secret for the current application.</param>
        /// <returns></returns>
        public QuickBloxSessionFactoryBuilder AuthorizationSecret(string authorizationSecret)
        {
            this.authorizationSecret = authorizationSecret;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="QuickBloxSessionFactory"/> instance.
        /// </summary>
        /// <returns>Returns a new <see cref="QuickBloxSessionFactory"/> instance.</returns>
        public QuickBloxSessionFactory BuildSessionFactory()
        {
            return new QuickBloxSessionFactory(quickBloxApiSessionUri, applicationId, authorizationKey, authorizationSecret);
        }
    }

    /// <summary>
    /// Holds all information needed for creating a new <see cref="QuickBloxSession"/>.
    /// </summary>
    public class QuickBloxSessionFactory
    {
        const string QuickBloxApiVersion = "0.1.0";

        Uri quickBloxApiUri;
        string applicationId;
        string authorizationKey;
        string authorizationSecret;

        internal QuickBloxSessionFactory(Uri quickBloxApiSessionUri, string applicationId, string authorizationKey, string authorizationSecret)
        {
            this.quickBloxApiUri = quickBloxApiSessionUri;
            this.applicationId = applicationId;
            this.authorizationKey = authorizationKey;
            this.authorizationSecret = authorizationSecret;
        }

        /// <summary>
        /// Opens a new <see cref="QuickBloxSession"/>.
        /// </summary>
        /// <returns>Returns QuickBlox session object for communicating with the QuickBlox' REST Api.</returns>
        public QuickBloxSession OpenSession()
        {
            var response = RequestNewQuickBloxSession();
            var session = ParseSessionCreationResponse(response.GetResponseStream());
            return session;
        }

        HttpWebResponse RequestNewQuickBloxSession()
        {
            StringBuilder requestDataBuilder = new StringBuilder();
            requestDataBuilder.AppendFormat("application_id={0}", applicationId);
            requestDataBuilder.AppendFormat("&auth_key={0}", authorizationKey);
            requestDataBuilder.AppendFormat("&nonce={0}", new Random().Next());
            requestDataBuilder.AppendFormat("&timestamp={0}", DateTime.UtcNow.ToUnixTimestamp());
            requestDataBuilder.AppendFormat("&signature={0}", ComputeSignature(authorizationSecret, requestDataBuilder.ToString()));

            byte[] requestData = new ASCIIEncoding().GetBytes(requestDataBuilder.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(quickBloxApiUri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = requestData.Length;
            request.Headers["QuickBlox-REST-API-Version"] = QuickBloxApiVersion;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(requestData, 0, requestData.Length);
            }
            return (HttpWebResponse)request.GetResponse();
        }

        string ComputeSignature(string secretKey, string data)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            HMACSHA1 hmacsha = new HMACSHA1(encoding.GetBytes(secretKey));
            byte[] bytes = encoding.GetBytes(data);
            return ByteToString(hmacsha.ComputeHash(bytes)).ToLowerInvariant();
        }

        QuickBloxSession ParseSessionCreationResponse(Stream sessionCreationResponse)
        {
            XDocument xmlDoc = XDocument.Load(sessionCreationResponse);

            return (from tutorial in xmlDoc.Descendants("session")
                    select new QuickBloxSession(tutorial.Element("token").Value))
                    .SingleOrDefault();

        }

        string ByteToString(byte[] buff)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buff.Length; i++)
            {
                sb.Append(buff[i].ToString("X2"));
            }
            return sb.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// QuickBlox session. This is the period of time used by API Users to interact with QuickBlox [[#API|API]. It's used to prevent transferring secretive data with each request. 
    /// Each session is identified by a session token. 
    /// </summary>
    public class QuickBloxSession
    {
        internal QuickBloxSession(string token)
        {
            Token = token;
        }

        /// <summary>
        /// Unique auto generated sequence of numbers which identify API User as the legitimate user of our system. It is used in relatively short periods of time and can be easily changed. 
        /// Grants API Users some rights after authentication and check them based on this token.
        /// </summary>
        public string Token { get; private set; }
    }

    internal static class DateTimeExtensions
    {
        static readonly DateTime unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Converts a <see cref="DateTime"/> object into a unix timestamp number.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>An intger for the number of seconds since 1st January 1970, as per unix specification.</returns>
        internal static int ToUnixTimestamp(this DateTime date)
        {
            TimeSpan ts = date - unixStartDate;
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// Converts a string, representing a unix timestamp number into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="timestamp">The timestamp, as a string.</param>
        /// <returns>The <see cref="DateTime"/> object the time represents.</returns>
        internal static DateTime UnixTimestampToDate(string timestamp)
        {
            if (timestamp == null || timestamp.Length == 0)
                return DateTime.MinValue;

            return UnixTimestampToDate(Int32.Parse(timestamp));
        }

        /// <summary>
        /// Converts a <see cref="long"/>, representing a unix timestamp number into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="timestamp">The unix timestamp.</param>
        /// <returns>The <see cref="DateTime"/> object the time represents.</returns>
        internal static DateTime UnixTimestampToDate(int timestamp)
        {
            return unixStartDate.AddSeconds(timestamp);
        }
    }
}