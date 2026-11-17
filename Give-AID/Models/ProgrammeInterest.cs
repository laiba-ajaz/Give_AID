using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    // Represents an expression of interest submitted by a user for a specific welfare programme
    public class ProgrammeInterest
    {
        // Primary key for the interest record
        [Key]
        public int InterestId { get; set; }

        // Foreign key referencing the targeted Programme
        [Required]
        public int ProgrammeId { get; set; }

        // Navigation property for the programme
        [ForeignKey(nameof(ProgrammeId))]
        public Programme? Programme { get; set; }

        // Foreign key referencing the registered User submitting interest
        [Required]
        public int UserId { get; set; }

        // Navigation property for the registered user
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Participant's full name
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Participant's email address
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Participant's phone number
        [StringLength(30)]
        public string? Phone { get; set; }

        // Selected participation type (e.g. Volunteer, Sponsor, OnlineMentor)
        [StringLength(50)]
        public string? ParticipationRole { get; set; } = "Volunteer";

        // Personal note or message explaining skills and background
        [StringLength(1000)]
        public string? Message { get; set; }

        // Timestamp when interest was registered
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
