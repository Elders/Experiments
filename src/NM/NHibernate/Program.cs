using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernateExperiments.DTOs;
using NHibernate.Linq;

namespace NHibernateExperiments
{
    class Program
    {
        static void Main(string[] args)
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            Bootstrapper.Boot();
            var sf = Bootstrapper.NHibernateSessionFactory;

            //  IDs
            var entityId = Guid.Parse("C4469189-9612-4CF3-89C9-EC71DD608202");

            ////  Insert data
            //using (var session = sf.OpenSession())
            //using (var tx = session.BeginTransaction())
            //{
            //    var entity = new Entity() { Id = entityId, Name = "EntityName" };
            //    session.Save(entity);
            //    tx.Commit();
            //}

            int global = 0;

            for (int i = 0; i < int.MaxValue; i++)
            {
                Console.WriteLine(i);
                using (var session = sf.OpenSession())
                using (var tx = session.BeginTransaction())
                {
                    var entity = session.Get<Entity>(entityId);
                    for (int j = 0; j < 1000; j++)
                    {
                        ++global;
                        Console.WriteLine(global);
                        entity.AddLoginStat(Guid.NewGuid());
                    }
                    tx.Commit();
                    i += 1000;
                }
            }

            Console.ReadLine();
        }
    }
}
