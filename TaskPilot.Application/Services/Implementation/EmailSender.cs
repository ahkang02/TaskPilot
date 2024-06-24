using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using TaskPilot.Application.Services.Interface;

namespace TaskPilot.Application.Services.Implementation
{
    public class EmailSender : IEmailSender
    {
        public string Sender { get; set; }
        public string SecretKey { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }

        public EmailSender(IConfiguration _config)
        {
            Sender = _config.GetValue<string>("SMTP:Sender")!;
            SecretKey = _config.GetValue<string>("SMTP:SecretKey")!;
            Host = _config.GetValue<string>("SMTP:Host")!;
            Port = _config.GetValue<string>("SMTP:Port")!;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Execute(email, subject, htmlMessage);
        }

        private async Task Execute(string email, string subject, string htmlMessage)
        {
            MailMessage mailMessage = new()
            {
                From = new MailAddress(Sender, "TaskPilot Support @MY"),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(email));

            SmtpClient smtpClient = new()
            {
                Host = Host,
                Port = Convert.ToInt32(Port),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Sender, SecretKey)
            };

            smtpClient.Send(mailMessage);
        }

    }
}
