using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iterator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int> { 1, 2, 3, 4 };
            var iterator = list.GetEnumerator();
            iterator.MoveNext();
            Console.WriteLine(iterator.Current);

            List<int> asd = new List<int>() { 1 };
            Console.WriteLine(asd.Last());
        }
    }
}
