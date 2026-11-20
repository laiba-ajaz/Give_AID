namespace Give_AID.Services
{
    // Interface for dispatching notification and invitation emails
    public interface IEmailSender
    {
        // Asynchronously sends an email to the specified recipient
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
