using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatsD
{
    class Program
    {
        static void Main(string[] args)
        {
            new Statsd(new XmlConfiguration());

            while (true)
            {
                Thread.Sleep(100);
                Statsd.Current.LogCount("mynkow");
            }


        }
    }
}
