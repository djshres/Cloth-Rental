using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace API.Controllers
{
    public class Mail
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }

        public Mail()
        {
            Host = "smtp.gmail.com";
            Port = "587";
            UseSsl = false;

            Username = "crossovercourier@gmail.com";
            Password = "dwshxxadjwtnpfdn";

            FromEmail = "Cloth Rental Serivce(P) ltd.|crossovercourier@gmail.com";

            try
            {
                UseSsl = false;
            }
            catch (Exception ex)
            {

            }
        }

        public bool Send(string to, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(to)) throw new Exception("To email address required.");
                if (string.IsNullOrEmpty(subject)) throw new Exception("Email subject required.");
                if (string.IsNullOrEmpty(body)) throw new Exception("Email message body required.");

                var message = new MimeMessage();

                var arrTemp = FromEmail.Split('|');
                message.From.Add(arrTemp.Length == 2
                    ? new MailboxAddress(arrTemp[0], arrTemp[1])
                    : new MailboxAddress("", arrTemp[0]));

                arrTemp = to.Split('|');
                message.To.Add(arrTemp.Length == 2
                    ? new MailboxAddress(arrTemp[0], arrTemp[1])
                    : new MailboxAddress("", arrTemp[0]));

                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Connect(Host, Convert.ToInt32(Port), UseSsl);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(Username, Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
