using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FluentEmail.Core;
using FluentEmail.Mailgun;

using SampleApi.Options;

namespace SampleApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly AppOptions _appOptions;

        public EmailService(IOptions<AppOptions> appOptions)
        {
            _appOptions = appOptions.Value;
            Email.DefaultSender = new MailgunSender(_appOptions.EmailProviders.Mailgun.Domain, _appOptions.EmailProviders.Mailgun.ApiKey);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = Email
            .From(_appOptions.EmailProviders.Mailgun.FromAddress, _appOptions.EmailProviders.Mailgun.FromName)
            .To(toEmail)
            .Subject(subject)
            .Body(message, true);
            var response = await email.SendAsync();
            var a = 10;
        }
    }
}