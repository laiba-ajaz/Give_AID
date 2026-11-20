namespace Give_AID.Services
{
    // Fake email sender for development and testing: logs simulated email dispatch to the console
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        // Constructor injecting ASP.NET Core logger
        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        // Logs simulated email dispatch details to the application console output
        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("================ SIMULATED EMAIL DISPATCH ================");
            _logger.LogInformation("TO: {ToEmail}", toEmail);
            _logger.LogInformation("SUBJECT: {Subject}", subject);
            _logger.LogInformation("BODY: {Body}", body);
            _logger.LogInformation("==========================================================");
            return Task.CompletedTask;
        }
    }
}
