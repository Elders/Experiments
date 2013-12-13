using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public class QuickBloxApiClient
    {
        const string QuickBloxErrors = "QuickBloxErrors";
        const string QuickBloxApiVersion = "0.1.0";

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QuickBloxApiClient));

        public static HttpWebResponse GetCustomObject(string sessionToken, string customObjectQuery)
        {
            HttpWebResponse response = null;
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}", "Get custom object(s).", "GET", customObjectQuery, QuickBloxApiVersion);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(customObjectQuery);
                request.Method = "GET";
                request.Headers["QB-Token"] = sessionToken;
                response = (HttpWebResponse)request.GetResponse();
                log.DebugFormat("[QuickBlox] - Response for '{0}' was '{1}'", customObjectQuery, response.StatusCode);
            }
            catch (WebException ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", customObjectQuery);
                log.LogWebException("QuickBlox", errorMessage, ex);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Warn(errorMessage, ex);
                throw new QuickBloxException(errorMessage, ex);
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", customObjectQuery);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Error(errorMessage, ex);
                throw ex;
            }
            return response;
        }

        public static HttpWebResponse RequestNewAnonymousSession(string sessionUrl, string applicationId, string authorizationKey, string authorizationSecret)
        {
            HttpWebResponse response = null;
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}\tApplicationId: {4}\tAuthorizationKey: {5}\tAuthorizationSecret: {6}*****", "Request new anonymous QuickBlox session.", "POST", sessionUrl, QuickBloxApiVersion, applicationId, authorizationKey, authorizationSecret.Substring(0, 5));
                StringBuilder requestDataBuilder = new StringBuilder();
                requestDataBuilder.AppendFormat("application_id={0}", applicationId);
                requestDataBuilder.AppendFormat("&auth_key={0}", authorizationKey);
                requestDataBuilder.AppendFormat("&nonce={0}", new Random().Next());
                requestDataBuilder.AppendFormat("&timestamp={0}", DateTime.UtcNow.ToUnixTimestamp());
                requestDataBuilder.AppendFormat("&signature={0}", ComputeSignature(authorizationSecret, requestDataBuilder.ToString()));

                byte[] requestData = new ASCIIEncoding().GetBytes(requestDataBuilder.ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sessionUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;
                request.Headers["QuickBlox-REST-API-Version"] = QuickBloxApiVersion;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
                response = (HttpWebResponse)request.GetResponse();
                log.DebugFormat("[QuickBlox] - Response for '{0}' was '{1}'", sessionUrl, response.StatusCode);
            }
            catch (WebException ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", sessionUrl);
                log.LogWebException("QuickBlox", errorMessage, ex);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Warn(errorMessage, ex);
                throw new QuickBloxException(errorMessage, ex);
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", sessionUrl);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Error(errorMessage, ex);
                throw ex;
            }
            return response;
        }

        public static HttpWebResponse RequestNewUserSession(string sessionUrl, string applicationId, string authorizationKey, string authorizationSecret, string username, string userPassword)
        {
            HttpWebResponse response = null;
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}\tApplicationId: {4}\tAuthorizationKey: {5}\tAuthorizationSecret: {6}*****\tUserLogin: {7}\tUserPassword: ******", "Request new user QuickBlox session.", "POST", sessionUrl, QuickBloxApiVersion, applicationId, authorizationKey, authorizationSecret.Substring(0, 5), username);
                StringBuilder requestDataBuilder = new StringBuilder();
                requestDataBuilder.AppendFormat("application_id={0}", applicationId);
                requestDataBuilder.AppendFormat("&auth_key={0}", authorizationKey);
                requestDataBuilder.AppendFormat("&nonce={0}", new Random().Next());
                requestDataBuilder.AppendFormat("&timestamp={0}", DateTime.UtcNow.ToUnixTimestamp());
                requestDataBuilder.AppendFormat("&user[login]={0}", username);
                requestDataBuilder.AppendFormat("&user[password]={0}", userPassword);
                requestDataBuilder.AppendFormat("&signature={0}", ComputeSignature(authorizationSecret, requestDataBuilder.ToString()));

                byte[] requestData = new ASCIIEncoding().GetBytes(requestDataBuilder.ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sessionUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;
                request.Headers["QuickBlox-REST-API-Version"] = QuickBloxApiVersion;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
                response = (HttpWebResponse)request.GetResponse();
                log.DebugFormat("[QuickBlox] - Response for '{0}' was '{1}'", sessionUrl, response.StatusCode);
            }
            catch (WebException ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", sessionUrl);
                log.LogWebException("QuickBlox", errorMessage, ex);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Warn(errorMessage, ex);
                throw new QuickBloxException(errorMessage, ex);
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", sessionUrl);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Error(errorMessage, ex);
                throw ex;
            }
            return response;
        }

        public static HttpWebResponse SaveCustomObject(string sessionToken, string customObjectUrl, string objectData)
        {
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}", "Save custom object: " + objectData, "POST", customObjectUrl, QuickBloxApiVersion);
                byte[] requestData = new ASCIIEncoding().GetBytes(objectData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(customObjectUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;
                request.Headers["QuickBlox-REST-API-Version"] = QuickBloxApiVersion;
                request.Headers["QB-Token"] = sessionToken;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("403"))
                    throw new WebException(String.Format("Did you forget to add a schema for QuickBlox custom object: '{0}'?", customObjectUrl), ex);
                else
                    throw ex;
            }
        }

        public static HttpWebResponse GetUserByExternalUserId(string sessionToken, string getUserByExternalUserIdQuery)
        {
            HttpWebResponse response = null;
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}", "Get custom object(s).", "GET", getUserByExternalUserIdQuery, QuickBloxApiVersion);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getUserByExternalUserIdQuery);
                request.Method = "GET";
                request.Headers["QB-Token"] = sessionToken;
                response = (HttpWebResponse)request.GetResponse();
                log.DebugFormat("[QuickBlox] - Response for '{0}' was '{1}'", getUserByExternalUserIdQuery, response.StatusCode);
            }
            catch (WebException ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", getUserByExternalUserIdQuery);
                log.LogWebException("QuickBlox", errorMessage, ex);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Warn(errorMessage, ex);
                throw new QuickBloxException(errorMessage, ex);
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[QuickBlox] - Error occured while querying {0}", getUserByExternalUserIdQuery);
                StatsdClient.Statsd.Current.LogQuickBloxCount(QuickBloxErrors);
                log.Error(errorMessage, ex);
                throw ex;
            }
            return response;
        }

        public static HttpWebResponse SaveCustomObjectsCollection(string sessionToken, string customObjectUrl, string objectsData)
        {
            try
            {
                log.DebugFormat("[QuickBlox] - {0}\tAction: {1}\tUrl: {2}\tApiVer: {3}", "Save custom object: " + objectsData, "POST", customObjectUrl, QuickBloxApiVersion);
                byte[] requestData = new ASCIIEncoding().GetBytes(objectsData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(customObjectUrl.Insert(customObjectUrl.LastIndexOf(".json"), "/multi"));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;
                request.Headers["QuickBlox-REST-API-Version"] = QuickBloxApiVersion;
                request.Headers["QB-Token"] = sessionToken;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("403"))
                    throw new WebException(String.Format("Did you forget to add a schema for QuickBlox custom object: '{0}'?", customObjectUrl), ex);
                else
                    throw ex;
            }
        }

        static string ByteToString(byte[] buff)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buff.Length; i++)
            {
                sb.Append(buff[i].ToString("X2"));
            }
            return sb.ToString().ToLowerInvariant();
        }

        static string ComputeSignature(string secretKey, string data)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            HMACSHA1 hmacsha = new HMACSHA1(encoding.GetBytes(secretKey));
            byte[] bytes = encoding.GetBytes(data);
            return ByteToString(hmacsha.ComputeHash(bytes)).ToLowerInvariant();
        }

    }
}