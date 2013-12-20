using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeSmtp;

namespace STMP
{

    public class SMTPServer
    {
        private static Timer tim = new Timer(Messages, listener, 0, 100);

        static MailListener listener = null;
        static void Main(string[] args)
        {


            do
            {
                Console.WriteLine("New MailListener started");
                if (listener == null)
                {
                }
                listener = new MailListener(new SMTPServer(),
                IPAddress.Loopback, 25673);

                listener.Start();
                while (listener.IsThreadAlive)
                {
                    Thread.Sleep(500);
                }
            } while (listener != null);
        }

        public static void Messages(object state)
        {
            Console.ReadLine();

            SmtpClient smtpClient = new SmtpClient(IPAddress.Loopback.MapToIPv4().ToString(), 25673);

            smtpClient.Credentials = new System.Net.NetworkCredential("no-reply@hyperion.com", "none");
            smtpClient.UseDefaultCredentials = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = false;
            MailMessage mail = new MailMessage();


            //Setting From , To and CC
            mail.From = new MailAddress("no-reply@hyperion.com", "hyperion org");
            mail.To.Add(new MailAddress("simeon.dimov@mentormate.com"));
            mail.Body = "test";

            smtpClient.Send(mail);
        }
    }

}
