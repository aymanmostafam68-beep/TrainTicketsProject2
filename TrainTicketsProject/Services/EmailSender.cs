using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Identity.UI.Services;

namespace TrainTicketsProject.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
            };

            return client.SendMailAsync(
                new MailMessage(from: _emailSettings.From,
                                to: email,
                                subject,
                                htmlMessage

                                )
                {
                    IsBodyHtml = true
                }

                );
        }
    }
}
