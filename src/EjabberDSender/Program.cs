using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Jabber;
using Jabber.Client;
using Jabber.Connection;
using Jabber.Protocol.Client;

namespace EjabberDSender
{
    class Program
    {
        public static ManualResetEvent handle;
        static JabberClient jc;
        public static string jizz;
        public static Thread newThread;
        static void Main(string[] args)
        {
            //newThread = new Thread(new ThreadStart(() =>
            //{
            jc = new JabberClient();
            jc.OnReadText += new Bedrock.TextHandler((sender, text) => Console.WriteLine(text));
            jc.OnWriteText += new Bedrock.TextHandler((sender, text) => Console.WriteLine(text));
            jc.OnError += new Bedrock.ExceptionHandler((sender, ex) => Console.WriteLine(ex.Message));
            jc.OnStreamError += new Jabber.Protocol.ProtocolHandler((sender, ex) => Console.WriteLine(ex.ToString()));
            jc.AutoReconnect = 3f;
            jc.OnInvalidCertificate += jc_OnInvalidCertificate;
            jc.User = Console.ReadLine();
            jc.Server = "jabber.devsmm.com";
            jc.NetworkHost = "192.168.1.46";
            jc.Port = 5222;
            jc.Resource = "Jabber.Net Console Client";
            jc.Password = "admin";
            jc.AutoStartTLS = true;
            jc.AutoPresence = true;
            jc.Connection = ConnectionType.Socket;
            jc["USE_WINDOWS_CREDS"] = false;
            jc["plaintext"] = false;
            jc["to"] = "jabber.devsmm.com";
            jc["network_host"] = "192.168.1.46"; //"jabber.devsmm.com";
            jc["ssl"] = false;
            jc["port"] = 5222;
            jc["poll.url"] = "";
            jc.OnMessage += jc_OnMessage;
            jc.OnAuthError += jc_OnAuthError;
            jc.OnAuthenticate += jc_OnAuthenticate;
            jc.OnConnect += jc_OnConnect;
            
            Console.WriteLine("Connecting");
            jc.Connect();
            Console.WriteLine("Connected");
            Console.ReadLine();
            Console.WriteLine("Subject:");
            var subject = Console.ReadLine();
            Console.WriteLine("Subject:" + subject);
            Console.WriteLine("Body:");
            var body = Console.ReadLine();
            Message msg = new Message(jc.Document);
            msg.To = "admin@" + jc.Server;
            //msg.From = new JID("kill@" + jc.Server);
            msg.Subject = subject;
            msg.Body = body;
            jc.Write(msg);

            Console.WriteLine("sent");
            Console.WriteLine("Closing");
            //}));
            //newThread.Start();

            //handle = new ManualResetEvent(false);
            //handle.WaitOne(Timeout.Infinite);
            Console.WriteLine("Closing");
            Console.ReadLine();
        }

        static void jc_OnConnect(object sender, StanzaStream stream)
        {

            Console.WriteLine("Connected");
        }

        static void jc_OnAuthenticate(object sender)
        {
            Console.WriteLine("Authenthicated");
        }

        static void jc_OnAuthError(object sender, XmlElement rp)
        {
            jc.Register(new JID(jc.User, jc.Server, null));

            jc.Connect();
            
        }

        static void jc_OnRegistered(object sender, IQ iq)
        {
            Console.WriteLine("Registered");
        }

        static void jc_OnMessage(object sender, Message msg)
        {
            Console.WriteLine(msg.From);
            Console.WriteLine("Subject:" + msg.Subject);
            Console.WriteLine("Body:" + msg.Body);
        }

        static bool jc_OnInvalidCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Invalid certificate");
            return true;
        }

    }
}
