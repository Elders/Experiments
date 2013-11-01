using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernateExperiments.DTOs;

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
            var entityId = Guid.NewGuid();
            var item1Id = Guid.NewGuid();
            var item2Id = Guid.NewGuid();


            //  Insert data
            using (var session = sf.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var entity = new Entity() { Id = entityId, Name = "EntityName" };
                var entityItem = new EntityItem() { Id = item1Id, Description = "First item" };
                entity.AddItem(entityItem);

                session.Save(entity);
                tx.Commit();
            }

            using (var session = sf.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var entity = session.Get<Entity>(entityId);
                var entityItem = new EntityItem() { Id = item2Id, Description = "Secind item" };
                entity.AddItem(entityItem);
                tx.Commit();
            }

            using (var session = sf.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var entity = session.Get<Entity>(entityId);
                var entityItem = new EntityItem() { Id = item2Id, Description = "Secind item" };
                entity.AddItem(entityItem);
                tx.Commit();
            }

            //  Remove item
            //using (var session = sf.OpenSession())
            //using (var tx = session.BeginTransaction())
            //{
            //    var entity = session.Get<Entity>(entityId);
            //    var entityItem = new EntityItem() { Id = item2Id, Description = "Secind item" };
            //    entity.RemoveItem(entityItem);
            //    tx.Commit();
            //}

            //  Delete parent entity
            //using (var session = sf.OpenSession())
            //using (var tx = session.BeginTransaction())
            //{
            //    var entity = session.Get<Entity>(entityId);
            //    session.Delete(entity);
            //    tx.Commit();
            //}

            Console.ReadLine();
        }
    }
}
