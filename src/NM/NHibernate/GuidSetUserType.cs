using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.UserTypes;
using NHibernate;
using NHibernate.SqlTypes;

namespace NHibernateExperiments
{
    public class GuidSetUserType : IUserType
    {
        private const char cStringSeparator = '#';

        bool IUserType.Equals(object x, object y)
        {
            if (x == null || y == null) return false;
            HashSet<Guid> xl = (HashSet<Guid>)x;
            HashSet<Guid> yl = (HashSet<Guid>)y;
            if (xl.Count != yl.Count) return false;
            Boolean retvalue = xl.Except(yl).Count() == 0;
            return retvalue;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            HashSet<Guid> obj = (HashSet<Guid>)value;
            HashSet<Guid> retvalue = new HashSet<Guid>(obj);

            return retvalue;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            HashSet<Guid> result = new HashSet<Guid>();
            Int32 index = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index) || String.IsNullOrEmpty((String)rs[index]))
                return result;
            foreach (String s in ((String)rs[index]).Split(cStringSeparator))
            {
                var id = Guid.Parse(s);
                result.Add(id);
            }
            return result;
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            if (value == null || value == DBNull.Value)
            {
                NHibernateUtil.StringClob.NullSafeSet(cmd, null, index);
            }
            HashSet<Guid> stringList = (HashSet<Guid>)value;
            StringBuilder sb = new StringBuilder();
            foreach (Guid s in stringList)
            {
                sb.Append(s);
                sb.Append(cStringSeparator);
            }
            if (sb.Length > 0) sb.Length--;
            NHibernateUtil.StringClob.Set(cmd, sb.ToString(), index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(ISet<String>); }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return new SqlType[] { NHibernateUtil.StringClob.SqlType }; }
        }
    }
}