using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadonlyCollections
{
    public static class MeasureExecutionTime
    {
        public static string Start(Action action)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action<int> action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action(i);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            IReadOnlyList<int> readonlyC = new ReadOnlyCollection<int>(new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var writable = readonlyC.ToList();
            writable[0] = 11;
            readonlyC = writable;

            ((IList<int>)readonlyC)[0] = 12;
            Console.ReadLine();
        }
    }
}