using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernateExperiments;
using NHibernateExperiments.DTOs;

namespace NHibernateExperiments
{
    public static class Bootstrapper
    {
        private static string connectionString;

        private static string databaseName;

        private static bool isNewInstance = false;

        private static ISessionFactory nhSessionFactory;

        public static ISessionFactory NHibernateSessionFactory
        {
            get { return nhSessionFactory; }
        }

        public static void Boot()
        {
            LoadConnectionStringAndDatabaseName();

            //DatabaseManager.DeleteDatabase(connectionString);

            isNewInstance = CreateDatabaseIfNotExists();

            nhSessionFactory = BuildNHibernateSessionFacotory();
        }

        private static ISessionFactory BuildNHibernateSessionFacotory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(Entity)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg.Properties[NHibernate.Cfg.Environment.CollectionTypeFactoryClass] = typeof(Sharetronix.Cfg.ForNHibernate.Net4CollectionTypeFactory).AssemblyQualifiedName;
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped, customClassMappings);

            if (isNewInstance)
                cfg.CreateDatabaseTables();

            return cfg.BuildSessionFactory();
        }

        static Action<ConventionModelMapper> customClassMappings = modelMapper =>
        {
            modelMapper.Class<Entity>(mapper =>
            {
                mapper.Property("QueryableLogins", prmap =>
                {
                    prmap.Type<GuidSetUserType>();
                    prmap.Length(4001);
                });
            });
        };

        private static bool CreateDatabaseIfNotExists()
        {
            if (!DatabaseManager.Exists(connectionString))
                return DatabaseManager.CreateDatabase(connectionString, enableSnapshotIsolation: true);

            return false;
        }

        private static void LoadConnectionStringAndDatabaseName()
        {
            var connStringSettings = ConfigurationManager.ConnectionStrings["nhExp"];
            if (connStringSettings == null)
                throw new ConfigurationErrorsException("Cannot find a connection string with name 'nhExp'");

            connectionString = connStringSettings.ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            databaseName = builder.InitialCatalog;
        }
    }
}