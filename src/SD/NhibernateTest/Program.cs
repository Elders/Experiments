using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg;
using System.Globalization;
using System.Reflection;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace NhibernateTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var nhSessionFactory = new NHibernate.Cfg.Configuration()
                                   //  .AddAutoMappings()
                                     .DropDatabaseTables()
                                     .CreateDatabaseTables()
                                     .BuildSessionFactory();
            var start = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                using (var session = nhSessionFactory.OpenSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        tx.Commit();
                    }

                }

            }
            var end = DateTime.Now;
            int breaasd = 0;
        }

    }
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Adds all Hyperion mappings to a NHibernate configuration.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance with Hyperion mappings.</returns>


        /// <summary>
        /// Drops the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration DropDatabaseTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Drop(false, true);
            return nhConf;
        }

        /// <summary>
        /// Drops and creates the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration CreateDatabaseTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Create(false, true);
            return nhConf;
        }
    }
}
