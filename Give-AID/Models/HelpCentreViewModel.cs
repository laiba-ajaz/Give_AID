using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // ViewModel for Help Centre query submissions and state feedback
    public class HelpCentreViewModel
    {
        // Inquirer's full name
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters.")]
        public string FullName { get; set; } = string.Empty;

        // Inquirer's email address
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Selected topic
        [Required(ErrorMessage = "Please select an inquiry topic.")]
        [StringLength(100)]
        public string Topic { get; set; } = "donation";

        // Urgent priority checkbox flag
        public bool IsUrgent { get; set; }

        // Detailed message text (minimum 10 characters per project specification)
        [Required(ErrorMessage = "Please describe your inquiry.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be at least 10 characters.")]
        public string Message { get; set; } = string.Empty;

        // Status flags for UI feedback
        public bool IsSuccess { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
