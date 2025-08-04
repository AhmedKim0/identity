namespace Identity.Infrastructure.EmailServices
{
    public interface IEmailService
    {
        Task<EmailMessage> GetEmailStructure(EmailStructure emailStructure, string emailAddress);
        EmailMessage ReplacePlaceholders(EmailMessage emailMessage, Dictionary<string, string> placeholders);
        Task SendEmailAsync(EmailMessage emailMessage);
    }
}