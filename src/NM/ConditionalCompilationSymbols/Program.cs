using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalCompilationSymbols
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.GetFiles(".", "*.txt").ToList().ForEach(f => File.Delete(f));
#if STAGE
            File.Create("Stage.txt");
#elif !DEBUG
            File.Create("Release.txt");
#else
            File.Create("Debug.txt");
#endif
        }
    }
}
