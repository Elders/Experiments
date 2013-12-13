using LinqProvider.LP;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaCore.Hyperion.Adapters.QuickBloxIntegration;
using LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg;
using System.Reflection;
using StatsdClient;
using System.Runtime.Serialization;
using IQToolkit;

namespace LinqProvider
{

    class Program
    {
        static void Main(string[] args)
        {
            new Statsd(new XmlConfiguration());
            quickbloxCfg = QuickBloxIntegrationConfiguration.Load(Assembly.GetAssembly(typeof(TextMessageSent)));
            var sf = BuildQuickBloxSessionFactory();
            var session = sf.GetCurrentSession();

            var stats = (from txt in session.Query<IQuickBloxCustomObject>(typeof(TextMessageSent))
                         where txt.Created_at >= DateTime.Now.AddDays(-1).ToUnixTimestamp() && txt.Created_at <= DateTime.Now.ToUnixTimestamp()
                         orderby txt.Created_at descending
                         select txt as IQuickBloxCustomObject)
                        .Take(100)
                        .Skip(0);

            var expre = ((IQuery<IQuickBloxCustomObject>)stats);

            var result = stats.ToList();
            Console.ReadLine();
        }
        static QuickBloxIntegrationConfiguration quickbloxCfg;
        private static QuickBloxSessionFactory BuildQuickBloxSessionFactory()
        {
            return new QuickBloxConfiguration(quickbloxCfg.QuickBlox, quickbloxCfg.EventToCustomObjectMappings).BuildSessionFactory();
        }
    }

    [DataContract(Namespace = "LaCore.Hyperion.Contracts.Events", Name = "ba0a1f97-6ff0-4acc-8bb2-11506e514d97")]
    public class TextMessageSent : IQuickBloxCustomObject
    {
        public TextMessageSent() { }

        public TextMessageSent(int messageId, int senderId, int recipientId, double timestamp)
        {
            MessageId = messageId;
            SenderId = senderId;
            RecipientId = recipientId;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public int MessageId { get; set; }

        [DataMember(Order = 2)]
        public int SenderId { get; set; }

        [DataMember(Order = 3)]
        public int RecipientId { get; set; }

        [DataMember(Order = 4)]
        public double Timestamp { get; set; }

        [DataMember(Order = 100)]
        public double Created_at { get; set; }

        [DataMember(Order = 101)]
        public int External_ID { get; set; }
    }

    [DataContract(Namespace = "LaCore.Hyperion.Contracts.Events", Name = "ba0a1f97-6ff0-4acc-8bb2-11506e514d97")]
    public class UserLoggedIn : IQuickBloxCustomObject
    {
        public UserLoggedIn() { }


        [DataMember(Order = 4)]
        public double Timestamp { get; set; }

        [DataMember(Order = 100)]
        public double Created_at { get; set; }

        [DataMember(Order = 101)]
        public int External_ID { get; set; }
    }
}
