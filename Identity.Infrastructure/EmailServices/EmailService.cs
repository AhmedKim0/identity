using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Identity.Domain.Entities;
using Identity.Application.Reposatory;
using Microsoft.EntityFrameworkCore;
using Identity.Application.DTO;
using Identity.Application.Int;


namespace Identity.Infrastructure.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAsyncRepository<EmailBody> _EmailBody;

        public EmailService(IConfiguration configuration, IAsyncRepository<EmailBody> emailBody)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _EmailBody = emailBody ?? throw new ArgumentNullException(nameof(emailBody));
        }




        public async Task<EmailMessage> GetEmailStructure(EmailStructure emailStructure, string emailAddress)
        {
            var emailConfig = await _EmailBody.Dbset().FirstOrDefaultAsync(x => x.Id == (int)emailStructure);
            EmailMessage email = new EmailMessage()
            {
                ToAddress = emailAddress,
                Subject = emailConfig.Subject,
                Body = emailConfig.Body,
                IsBodyHtml = true

            };
            return email;

        }
        public EmailMessage ReplacePlaceholders(EmailMessage emailMessage, Dictionary<string, string> placeholders)
        {
            if (emailMessage?.Body == null || placeholders == null || placeholders.Count == 0)
                return emailMessage;

            foreach (var kvp in placeholders)
            {
                emailMessage.Body = emailMessage.Body.Replace($"{{{{{{{kvp.Key}}}}}}}", kvp.Value); // Matches {{{Key}}}
            }

            return emailMessage;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                using var smtp = new SmtpClient
                {
                    Host = _configuration["Email:SmtpServer"],
                    Port = int.Parse(_configuration["Email:Port"]),
                    EnableSsl = bool.Parse(_configuration["Email:EnableSsl"]),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]
                    )
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:SenderEmail"]),
                    Subject = emailMessage.Subject,
                    Body = emailMessage.Body,
                    IsBodyHtml = emailMessage.IsBodyHtml
                };

                message.To.Add(emailMessage.ToAddress);

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to send email.");
                throw; 
            }
        }
    }

}
