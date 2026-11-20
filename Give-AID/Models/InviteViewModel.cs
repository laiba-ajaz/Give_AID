using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // ViewModel for processing friend invitations and providing UI feedback
    public class InviteViewModel
    {
        // Staged list of recipient email addresses submitted by the user
        public List<string> Emails { get; set; } = new();

        // Personalized endorsement message
        [StringLength(1000, ErrorMessage = "Personal message cannot exceed 1000 characters.")]
        public string? Message { get; set; }

        // State feedback indicators
        public bool IsSuccess { get; set; }
        public int SentCount { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
