using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // ViewModel for rendering programmes, filtering initiatives, and handling interest form submissions
    public class ProgrammeIndexViewModel
    {
        // Collection of all active programmes loaded from the database
        public List<Programme> ActiveProgrammes { get; set; } = new();

        // Selected programme ID for registration
        [Required(ErrorMessage = "Please select a programme.")]
        public int ProgrammeId { get; set; }

        // Participant full name
        [Required(ErrorMessage = "Please provide your full name.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters.")]
        public string FullName { get; set; } = string.Empty;

        // Participant email address
        [Required(ErrorMessage = "Please provide your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email format.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Participant phone number
        [Required(ErrorMessage = "Please provide your contact phone number.")]
        [StringLength(30, MinimumLength = 7, ErrorMessage = "Phone number must be at least 7 digits.")]
        public string Phone { get; set; } = string.Empty;

        // Role: Volunteer, Sponsor, OnlineMentor
        public string ParticipationRole { get; set; } = "Volunteer";

        // Personal statement or experience
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string? Message { get; set; }

        // Confirmation of background check agreement
        public bool AgreeBackgroundCheck { get; set; }

        // Status flags for view feedback
        public bool IsSuccess { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
