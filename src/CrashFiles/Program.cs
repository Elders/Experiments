using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrashFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            int x = 10017;
            var arry = Convert.ToString(x, 2);
            Console.WriteLine(arry);
            Console.ReadLine();
            //var bytes = File.ReadAllBytes("crash.txt");
            //var byte2s = new List<byte>() { byte.Parse("113") };
            //byte2s.AddRange(bytes);
            //File.WriteAllBytes("crashed.txt", byte2s.ToArray());
        }
    }
}
