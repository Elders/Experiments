using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Cronus.Core.Eventing;
using LaCore.Hyperion.Adapters.QuickBloxIntegration.ElasticComputing;
using LaCore.Hyperion.Adapters.QuickBloxIntegration.Eventing;
using LaCore.Hyperion.Contracts.Events;
using LaCore.Hyperion.Infrastructure;
using LaCore.Hyperion.Infrastructure.MassTransit;
using LaCore.Hyperion.Infrastructure.MassTransit.ElasticComputing;
using LaCore.Hyperion.Infrastructure.MassTransit.ElasticComputing.Work;
using LaCore.Hyperion.Ports.DTOs;
using MassTransit;
using NHibernate;
using NHibernate.Mapping.ByCode;
using Protoreg;
using StatsdClient;
using LaCore.Hyperion.Infrastructure.NHibernateExtensions;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration.Cfg
{
    public static class QuickBloxIntegrationBootstrapper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QuickBloxIntegrationBootstrapper));

        private static string connectionString;

        private static string databaseName;

        private static IServiceBus eventStoreRecievingBus;

        private static QuickBloxIntegrationConfiguration quickbloxCfg;

        private static bool isNewInstance = false;

        private static PersistentEventSource marketVisionQueue;

        private static IServiceBus messageBus;

        private static ISessionFactory nhSessionFactory;

        private static ProtoregSerializer protoregSerializer;

        private static IQuickBloxSessionFactory qbSessionFacotry;

        private static QuickBloxElasticPuller quickBloxCrawler;

        public static QuickBloxIntegrationConfiguration QuickBlobConfiguration
        {
            get { return quickbloxCfg; }
        }

        public static IServiceBus MessageBus
        {
            get { return messageBus; }
        }

        public static ISessionFactory NHibernateSessionFactory
        {
            get { return nhSessionFactory; }
        }

        public static QuickBloxElasticPuller QuickBloxCrawler
        {
            get { return quickBloxCrawler; }
        }

        public static IQuickBloxSessionFactory QuickBloxSessionFactory
        {
            get { return qbSessionFacotry; }
        }

        public static void Boot()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                log.Debug("[QuickBlox] - log4net was configured.");

                LoadConnectionStringAndDatabaseName();
                log.DebugFormat("[QuickBlox] - Connection string loaded: '{0}'.", connectionString);

#if DEBUG
                DatabaseManager.DeleteDatabase(connectionString);
                Process.Start(@"C:\Program Files (x86)\RabbitMQ Server\rabbitmq_server-3.1.5\sbin\rabbitmqctl.bat", "stop_app");
                Thread.Sleep(1000);
                Process.Start(@"C:\Program Files (x86)\RabbitMQ Server\rabbitmq_server-3.1.5\sbin\rabbitmqctl.bat", "reset");
                Thread.Sleep(1000);
                Process.Start(@"C:\Program Files (x86)\RabbitMQ Server\rabbitmq_server-3.1.5\sbin\rabbitmqctl.bat", "start_app");
                Thread.Sleep(1000);
#endif

                isNewInstance = CreateDatabaseIfNotExists();
                log.Debug("[QuickBlox] - Database initialized.");

                protoregSerializer = BuildProtoregSerializer();
                log.Debug("[QuickBlox] - Protoreg Serializer was configured.");

                nhSessionFactory = BuildNHibernateSessionFacotory();
                log.Debug("[QuickBlox] - NHibernate SessionFactory was configured.");

                if (isNewInstance)
                {
                    log4net.Config.XmlConfigurator.Configure();
                    log.Debug("[QuickBlox] - log4net was configured.");
                }

                new Statsd(new XmlConfiguration());
                log.Debug("[QuickBlox] - StatsD was configured.");

                quickbloxCfg = QuickBloxIntegrationConfiguration.Load(Assembly.GetAssembly(typeof(TextMessageSent)));
                log.Debug("[QuickBlox] - QuickBloxIntegration was configured.");

                qbSessionFacotry = BuildQuickBloxSessionFactory();
                log.Debug("[QuickBlox] - QuickBloxSessionFactory was configured.");

                messageBus = BuildMessageBus();
                log.Debug("[QuickBlox] - MassTransit MessageBus was configured.");

                marketVisionQueue = new PersistentEventSource(PersistentEventSource.ProjectionsQueueName, protoregSerializer);

                eventStoreRecievingBus = BuildEventStoreMessageBus();
                log.Debug("[QuickBlox] - EventStore ConsumingBus was configured.");

                quickBloxCrawler = BuildQuickBloxCrawler();
                log.Debug("[QuickBlox] - QuickBloxCrawler was configured.");
            }
            catch (Exception ex)
            {
                log.Fatal("[QuickBlox] - Cannot boot the service", ex);
                throw ex;
            }
        }

        private static IServiceBus BuildEventStoreMessageBus()
        {
            bool shouldPurge = isNewInstance;
            string zeroQuickBloxIntegrationQueueName = String.Format("rabbitmq://localhost/Hyperion-{0}-QuickBloxEventStore", databaseName);
            return ServiceBusFactory.New(sbc =>
            {
                sbc.ReceiveFrom(zeroQuickBloxIntegrationQueueName);
                sbc.UseRabbitMq();
                sbc.SetPurgeOnStartup(shouldPurge);
                sbc.SetDefaultSerializer(new MassTransitSerializer(protoregSerializer));
                sbc.Subscribe(subs =>
                {
                    subs.Consumer(typeof(EventStoreConsumer), x => new EventStoreConsumer(nhSessionFactory)).Permanent();
                });
            });
        }

        private static IServiceBus BuildMessageBus()
        {
            bool shouldPurge = isNewInstance;
            string zeroQuickBloxIntegrationQueueName = String.Format("rabbitmq://localhost/Hyperion-{0}-ZeroQuickBloxIntegration", databaseName);
            return ServiceBusFactory.New(sbc =>
            {
                sbc.ReceiveFrom(zeroQuickBloxIntegrationQueueName);
                sbc.UseRabbitMq();
                sbc.SetPurgeOnStartup(shouldPurge);
                sbc.SetDefaultSerializer(new MassTransitSerializer(protoregSerializer));
            });
        }

        private static ISessionFactory BuildNHibernateSessionFacotory()
        {
            var typesThatShouldBeMapped = new List<Type>();
            typesThatShouldBeMapped.Add(typeof(CommittedEvents));
            typesThatShouldBeMapped.Add(typeof(LaCore.Hyperion.Ports.DTOs.Log));

            BinaryWithProtobufUserType.Serializer = protoregSerializer;
            var cfg = new NHibernate.Cfg.Configuration().AddAutoMappings(typesThatShouldBeMapped, customClassMappings);

            if (isNewInstance)
                cfg.CreateDatabaseTables();

            return cfg.BuildSessionFactory();
        }

        static Action<ConventionModelMapper> customClassMappings = modelMapper =>
        {
            modelMapper.Class<CommittedEvents>(mapper =>
            {
                mapper.Property(pr => pr.Events, prmap => prmap.Type<BinaryWithProtobufUserType>());
            });

            modelMapper.Class<Log>(mapper =>
            {
                mapper.Property(pr => pr.Message, prmap => prmap.Length(4001));
                mapper.Property(pr => pr.Exception, prmap => prmap.Length(4001));
            });
        };

        private static QuickBloxElasticPuller BuildQuickBloxCrawler()
        {
            return new QuickBloxElasticPuller(
            QuickBlobConfiguration,
            QuickBloxSessionFactory,
            protoregSerializer, isNewInstance, new NextGenWorkProcessorFactory(QuickBloxSessionFactory, MessageBus, protoregSerializer));
        }

        private static ProtoregSerializer BuildProtoregSerializer()
        {
            var registration = new ProtoRegistration();
            registration.RegisterAssembly<TextMessageSent>();
            registration.RegisterAssembly<WorkManager>();
            registration.RegisterAssembly<PullFromQuickBlox>();
            // registration.RegisterCommonType<IEvent>();
            registration.RegisterCommonType<List<IEvent>>();
            registration.RegisterCommonType<Queue<Guid>>();
            registration.RegisterCommonType<Fault<UncommittedEvents>>();
            registration.RegisterCommonType<Fault<UserLoggedIn>>();
            registration.RegisterCommonType<Fault<BrokenEvent>>();
            registration.RegisterCommonType<Fault<AsyncWorkDone>>();
            registration.RegisterCommonType<Fault<PendingWork>>();
            registration.RegisterCommonType<Fault<IEvent>>();
            var serializer = new ProtoregSerializer(registration);
            serializer.Build();
            return serializer;
        }

        private static IQuickBloxSessionFactory BuildQuickBloxSessionFactory()
        {
            return new QuickBloxConfiguration(quickbloxCfg.QuickBlox, quickbloxCfg.EventToCustomObjectMappings).BuildSessionFactory();
        }

        private static bool CreateDatabaseIfNotExists()
        {
            if (!DatabaseManager.Exists(connectionString))
                return DatabaseManager.CreateDatabase(connectionString, enableSnapshotIsolation: true);

            return false;
        }

        private static void LoadConnectionStringAndDatabaseName()
        {
            var connStringSettings = ConfigurationManager.ConnectionStrings["hesConnection"];
            if (connStringSettings == null)
                throw new ConfigurationErrorsException("Cannot find a connection string with name 'hesConnection'");

            connectionString = connStringSettings.ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            databaseName = builder.InitialCatalog;
        }
    }
}