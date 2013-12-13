using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernateExperiments.DTOs
{
    public class Entity
    {
        public Entity()
        {
            QueryableLogins = new HashSet<Guid>();
        }

        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Text { get; set; }

        protected virtual ISet<Guid> QueryableLogins { get; set; }

        IReadOnlyCollection<Guid> readonlyItems;
        public virtual IReadOnlyCollection<Guid> Logins
        {
            get
            {
                if (readonlyItems == null || readonlyItems.Count != QueryableLogins.Count)
                {
                    readonlyItems = new ReadOnlyCollection<Guid>(QueryableLogins.ToList());
                }
                return readonlyItems;
            }
        }

        public virtual bool AddLoginStat(Guid loginStatId)
        {
            if (loginStatId == default(Guid))
                return false;

            return QueryableLogins.Add(loginStatId);
        }

        public virtual bool RemoveLoginStat(Guid loginStatId)
        {
            if (loginStatId == default(Guid))
                return false;

            return QueryableLogins.Remove(loginStatId);
        }

    }

    public class EntityItem : IEquatable<EntityItem>
    {
        public virtual Guid Id { get; set; }

        public virtual string Description { get; set; }

        public virtual Entity Parent { get; set; }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            EntityItem p = obj as EntityItem;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Id == p.Id);
        }

        public virtual bool Equals(EntityItem p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Id == p.Id);
        }

        public override int GetHashCode()
        {
            return 23 ^ Id.GetHashCode();
        }
    }
}
