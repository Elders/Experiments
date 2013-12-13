using LinqProvider.LP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using IQToolkit;
using LinqProvider;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public interface IQuickBloxCustomObject
    {
        double Created_at { get; set; }
    }

    /// <summary>
    /// QuickBlox session. This is the period of time used by API Users to interact with QuickBlox [[#API|API]. It's used to prevent transferring secretive data with each request. 
    /// Each session is identified by a session token. 
    /// </summary>
    public class QuickBloxSession
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QuickBloxSession));

        static Type quickBloxJsonResultWrapper = typeof(QuickBloxJsonResultWrapper);

        internal QuickBloxSession(string token)
        {
            Token = token;
        }

        private string compiledQuery { get; set; }

        public QuickBloxSessionFactory SessionFactory { get; set; }

        /// <summary>
        /// Unique auto generated sequence of numbers which identify API User as the legitimate user of our system. It is used in relatively short periods of time and can be easily changed. 
        /// Grants API Users some rights after authentication and check them based on this token.
        /// </summary>
        public string Token { get; private set; }

        //public QuickBloxSession Query(Type objectType, Action<IQuickBloxQueryBuilderNonGeneric> queryBuilder)
        //{
        //    this.objectType = objectType;
        //    var quickBloxQueryBuilder = new QuickBloxQueryBuilderNonGeneric(SessionFactory.EventToCustomObjectMappings[objectType], objectType);
        //    queryBuilder(quickBloxQueryBuilder);
        //    quickBloxQueryBuilder.CompileQuery();
        //    compiledQuery = quickBloxQueryBuilder.CustomObjectQuery;
        //    return this;
        //}


        public IQuery<T> Query<T>()
        {
            return new Query<T>(SessionFactory.Provider, typeof(T));
        }

        public IQuery<T> Query<T>(Type type)
        {
            var d1 = typeof(Query<>);
            Type[] typeArgs = { type };
            var makeme = d1.MakeGenericType(typeArgs);
            return (IQuery<T>)Activator.CreateInstance(makeme, new object[] { SessionFactory.Provider, makeme });
        }

        public void Save(object obj)
        {
            try
            {
                if (obj is IList)
                {
                    var collection = (IList)obj;
                    SaveCollectionInternal(collection);
                }
                else
                {
                    SaveInternal(obj);
                }
            }
            catch (Exception ex)
            {
                log.Error("[QuickBlox] - Error while saving QuickBlox custom object.", ex);
                throw ex;
            }
        }

        void SaveCollectionInternal(IList collection)
        {
            if (collection.Count == 0) return;
            Type objType = collection[0].GetType();

            StringBuilder postBodyBuilder = new StringBuilder();
            for (int i = 0; i < collection.Count; i++)
            {
                var currentItem = collection[i];
                var properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

                foreach (var prop in properties)
                {
                    postBodyBuilder.AppendFormat("&record[{0}][{1}]={2}", i, prop.Name, prop.GetValue(currentItem));
                }
            }

            var postBody = postBodyBuilder.ToString().TrimStart(new[] { '&' });

            var response = QuickBloxApiClient.SaveCustomObjectsCollection(Token, SessionFactory.EventToCustomObjectMappings[objType], postBody);
            response.Close();
        }

        void SaveInternal(object obj)
        {
            var customObjectType = obj.GetType();
            var members = customObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            StringBuilder postBodyBuilder = new StringBuilder();
            foreach (var prop in members)
            {
                postBodyBuilder.AppendFormat("&{0}={1}", prop.Name, prop.GetValue(obj));
            }
            var postBody = postBodyBuilder.ToString().TrimStart(new[] { '&' });

            var response = QuickBloxApiClient.SaveCustomObject(Token, SessionFactory.EventToCustomObjectMappings[obj.GetType()], postBody);
            response.Close();
        }

        class QuickBloxJsonResultWrapper
        {
            public IList Items { get; set; }
        }
    }
}