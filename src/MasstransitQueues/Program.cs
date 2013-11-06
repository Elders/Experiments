using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace MasstransitQueues
{
    class Program
    {//192.168.16.46
        static void Main(string[] args)
        {
            var serviceBus = ServiceBusFactory.New(x =>
                {
                    x.ReceiveFrom("rabbitmq://guest:guest@localhost/Sender");
                    x.UseRabbitMq();
                    x.Subscribe(sbc => sbc.Consumer<Asd>().Permanent());
                });
            //for (int i = 0; i < 50; i++)
            //{
            //    //  serviceBus.Publish(new mymsg() { Txt = "ad" });
            //    //serviceBus.GetEndpoint(new Uri("rabbitmq://guest:guest@192.168.16.46/Sender")).Receive(x => 
            //    {
            //        //serviceBus.ser
            //       .. return y => y.BodyStream;
            //    });//.Send(new mymsg() { Txt = "ad" });
            //}

            Thread.Sleep(4000);

            Console.ReadLine();
        }
    }

    public class mymsg
    {
        public string Txt { get; set; }
    }

    public class Asd : Consumes<mymsg>.All
    {

        public void Consume(mymsg message)
        {
            Console.WriteLine(message.Txt);
        }
    }
}
