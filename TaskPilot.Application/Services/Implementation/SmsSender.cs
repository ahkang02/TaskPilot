using Microsoft.Extensions.Configuration;
using TaskPilot.Application.Services.Interface;
using Vonage.Messaging;
using Vonage.Request;

namespace TaskPilot.Application.Services.Implementation
{
    public class SmsSender : ISmsSender
    {
        public string Account { get; set; }
        public string SecretKey { get; set; }
        public string Sender { get; set; }


        public SmsSender(IConfiguration _config)
        {
            Sender = _config.GetValue<string>("Vonage:Sender")!;
            Account = _config.GetValue<string>("Vonage:Account")!;
            SecretKey = _config.GetValue<string>("Vonage:SecretKey")!;
        }

        public async Task SendSmsAsync(string to, string body, string text)
        {
            await Execute(to, body, text);
        }

        private async Task Execute(string to, string body, string text)
        {
            var credentials = Credentials.FromApiKeyAndSecret(Account, SecretKey);
            var client = new SmsClient(credentials);
            var request = new SendSmsRequest { From = Sender, To = to, Body = body, Text = text };
            await client.SendAnSmsAsync(request);
        }
    }
}
