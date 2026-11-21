namespace Give_AID.Models
{
    // Configuration model for SMTP email delivery (Brevo / Sendinblue)
    public class SmtpSettings
    {
        public string Server { get; set; } = "smtp-relay.brevo.com";
        public int Port { get; set; } = 587;
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string SenderEmail { get; set; } = "";
        public string SenderName { get; set; } = "Give-AID Foundation";
    }
}
