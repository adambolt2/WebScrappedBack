using System;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace WebScrappedBack.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string to, string subject, string body);
    }

    public class MailjetService : IEmailService
    {
        private readonly MailjetOptions _mailjetOptions;

        public MailjetService(IOptions<MailjetOptions> mailjetOptions)
        {
            _mailjetOptions = mailjetOptions.Value;
        }

        public async Task SendVerificationEmailAsync(string to, string subject, string body)
        {
            var client = new MailjetClient(_mailjetOptions.ApiKey, _mailjetOptions.ApiSecret);

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("adameltarzi@gmail.com")) // Replace with your email
                .WithSubject(subject)
                .WithHtmlPart(body)
                .WithTo(new SendContact(to))
                .Build();

            var response = await client.SendTransactionalEmailAsync(email);

            if (response.Messages.Length != 1 || response.Messages[0].Status != "success")
            {
                throw new Exception($"Failed to send email: {response.Messages[0].Status}");
            }
        }
    }

    public class MailjetOptions
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}
