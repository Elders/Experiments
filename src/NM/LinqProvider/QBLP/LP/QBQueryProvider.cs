using LaCore.Hyperion.Adapters.QuickBloxIntegration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IQToolkit;

namespace LinqProvider.LP
{
    public class QuickBloxQueryProvider : QueryProvider
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QuickBloxQueryProvider));
        private readonly string quickBloxEndPoint;

        static Type quickBloxJsonResultWrapper = typeof(QuickBloxJsonResultWrapper);

        private readonly string sessionToken;

        public QuickBloxQueryProvider(string quickBloxEndPoint, string sessionToken)
        {
            this.sessionToken = sessionToken;
            this.quickBloxEndPoint = quickBloxEndPoint;
        }

        public override object Execute(Expression expression)
        {
            Type elementType = expression.GetElementType();
            IList<IQuickBloxCustomObject> customObjectsList = new List<IQuickBloxCustomObject>();
            var response = QuickBloxApiClient.GetCustomObject(sessionToken, this.Translate(expression));
            QuickBloxJsonResultWrapper deserializedObject;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var serializer = new JavaScriptSerializer();
                var json = reader.ReadToEnd();
                deserializedObject = (QuickBloxJsonResultWrapper)serializer.Deserialize(json, quickBloxJsonResultWrapper);
                response.Close();

                foreach (var item in deserializedObject.Items)
                {
                    IQuickBloxCustomObject inner = null;
                    try
                    {
                        inner = serializer.ConvertToType(item, elementType) as IQuickBloxCustomObject;
                        customObjectsList.Add(inner);
                    }
                    catch (InvalidOperationException ex)
                    {
                        var brokenJson = serializer.Serialize(item);
                        var errorMessage = String.Format("[QuickBlox] - Unable to deserialize to object. Type: {0} Json: {1}", elementType.FullName, brokenJson);
                        log.Warn(errorMessage, ex);
                        try
                        {

                            var brokenEvent = serializer.ConvertToType(item, typeof(BrokenEvent)) as IQuickBloxCustomObject;
                            (brokenEvent as BrokenEvent).Json = brokenJson;
                            (brokenEvent as BrokenEvent).EventType = elementType;
                            customObjectsList.Add(brokenEvent);
                        }
                        catch (Exception)
                        {
                            customObjectsList.Add(new BrokenEvent() { EventType = elementType, Json = brokenJson, Created_at = default(double) });
                        }
                        continue;
                    }
                };
                return customObjectsList;
            }
        }

        public override string GetQueryText(Expression expression)
        {
            return this.Translate(expression);
        }

        private string Translate(Expression expression)
        {
            expression = PartialEvaluator.Eval(expression);
            return new QuickBloxQueryTranslator(quickBloxEndPoint).Translate(expression);
        }

        class QuickBloxJsonResultWrapper
        {
            public System.Collections.IList Items { get; set; }
        }

    }
}
