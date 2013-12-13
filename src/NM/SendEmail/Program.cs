using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add("hyperion@mentormate.com");
            message.Subject = "aaaaaaaaaa";
            message.From = new System.Net.Mail.MailAddress("hyperion-noreply@mentormate.com");
            message.Body = "aaaaaaaaaaa";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("192.168.0.80");
            smtp.Send(message);
        }
    }
}
