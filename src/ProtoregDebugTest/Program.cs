using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Protoreg;

namespace ProtoregDebugTest
{
    class Program
    {

        static void Main(string[] args)
        {
            var reg = new ProtoRegistration();
            var str = new MemoryStream();
            var deleg = (Func<string, string>)Delegate.Combine(new Func<string, string>((x) => { Console.WriteLine("deleg 1" + x); return x; }), new Func<string, string>((x) => { Console.WriteLine("deleg 2" + x); return x; }));
            deleg(1.ToString());
            Console.Read();
        }
    }
}
