using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net
{
    class Program
    {


        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var result = MeasureExecutionTime.Start(() => new MyClass(), 1);
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }

    public class MyClass
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MyClass));
        int aaaa = 1;
        public MyClass()
        {
            try
            {
                throw new Exception("heeeeeeeeeeeeey");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

        }
    }

    public static class MeasureExecutionTime
    {
        public static string Start(Action action)
        {
            string result = string.Empty;
#if DEBUG
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
#endif
            return result;
        }

        public static string Start(Action action, int repeat)
        {
            string result = string.Empty;
#if DEBUG
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
#endif
            return result;
        }

        public static string Start(Action<int> action, int repeat)
        {
            string result = string.Empty;
#if DEBUG
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action(i);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
#endif
            return result;
        }
    }
}
