using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Give_AID.Models;

namespace Give_AID.Services
{
    // Real email dispatch service powered by Brevo SMTP relay
    public class BrevoEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<BrevoEmailSender> _logger;

        // Constructor injecting SMTP configuration and application logger
        public BrevoEmailSender(IOptions<SmtpSettings> settings, ILogger<BrevoEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // Sends formatted HTML invitation email to the specified recipient
        public async Task SendEmailAsync(string toEmail, string subject, string messageContent)
        {
            try
            {
                // Verify required SMTP settings are present
                if (string.IsNullOrWhiteSpace(_settings.Server) ||
                    string.IsNullOrWhiteSpace(_settings.Login) ||
                    string.IsNullOrWhiteSpace(_settings.Password) ||
                    string.IsNullOrWhiteSpace(_settings.SenderEmail))
                {
                    _logger.LogWarning("Brevo SMTP credentials are incomplete in appsettings.json. Logging to console instead.");
                    _logger.LogInformation("TO: {ToEmail} | SUBJECT: {Subject} | MESSAGE: {Message}", toEmail, subject, messageContent);
                    return;
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = BuildHtmlEmailBody(messageContent),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_settings.Server, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Login, _settings.Password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000 // 15 second timeout
                };

                _logger.LogInformation("Dispatching Brevo email to {ToEmail}...", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully dispatched to {ToEmail} via Brevo SMTP relay.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} via Brevo SMTP relay: {ErrorMessage}", toEmail, ex.Message);
            }
        }

        // Generates clean, branded HTML email template for invitations
        private static string BuildHtmlEmailBody(string personalMessage)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Invitation to Give-AID</title>
</head>
<body style=""margin:0; padding:0; background-color:#f1f4f9; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color:#181c20;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#f1f4f9; padding:30px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" style=""max-width:600px; background-color:#ffffff; border-radius:16px; overflow:hidden; box-shadow:0 4px 16px rgba(11,69,69,0.08);"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                    <!-- Brand Header -->
                    <tr>
                        <td style=""background-color:#002d2e; padding:32px 30px; text-align:center;"">
                            <h1 style=""margin:0; font-size:28px; font-weight:bold; color:#ffffff; letter-spacing:-0.5px;"">
                                Give<span style=""color:#fe7952;"">-AID</span>
                            </h1>
                            <p style=""margin:6px 0 0 0; font-size:13px; color:#7fb2b1; text-transform:uppercase; letter-spacing:1.5px; font-weight:600;"">
                                Global Welfare Foundation
                            </p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style=""padding:40px 35px;"">
                            <span style=""display:inline-block; background-color:#ffdbd1; color:#a73918; padding:4px 12px; border-radius:50px; font-size:12px; font-weight:bold; text-transform:uppercase; letter-spacing:1px; margin-bottom:16px;"">
                                Community Invitation
                            </span>

                            <h2 style=""margin:0 0 16px 0; font-size:22px; font-weight:bold; color:#002d2e; line-height:1.35;"">
                                Kindness multiplies when shared.
                            </h2>

                            <p style=""font-size:15px; line-height:1.6; color:#404848; margin:0 0 24px 0;"">
                                You have been personally invited by a friend to join the <strong>Give-AID</strong> community. Together, our giving circle supports vetted grassroots NGOs providing emergency relief, clean water, healthcare, and education to underserved communities worldwide.
                            </p>

                            <!-- Personal Endorsement Box -->
                            <div style=""background-color:#f7f9ff; border-left:4px solid #fe7952; padding:18px 20px; border-radius:8px; margin:0 0 28px 0;"">
                                <p style=""margin:0; font-size:14px; font-style:italic; color:#181c20; line-height:1.6;"">
                                    &ldquo;{WebUtility.HtmlEncode(personalMessage)}&rdquo;
                                </p>
                            </div>

                            <!-- CTA Button -->
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin-bottom:30px;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""http://localhost:5002/Programmes"" style=""display:inline-block; background-color:#a73918; color:#ffffff; text-decoration:none; padding:14px 32px; border-radius:10px; font-size:15px; font-weight:bold; letter-spacing:0.5px; text-transform:uppercase; box-shadow:0 2px 6px rgba(167,57,24,0.3);"">
                                            Explore Verified Programmes &rarr;
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style=""font-size:13px; line-height:1.5; color:#707978; margin:0;"">
                                Every donation on Give-AID is backed by 100% transparency with direct field reporting from our partner NGOs.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color:#ebeef3; padding:22px 30px; text-align:center; font-size:12px; color:#707978;"">
                            <p style=""margin:0 0 6px 0;"">
                                &copy; {DateTime.Now.Year} Give-AID Global Welfare Foundation. UN ECOSOC Accredited.
                            </p>
                            <p style=""margin:0;"">
                                You received this invitation upon the direct request of a registered member.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
