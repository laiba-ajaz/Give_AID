using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    // Database model representing a friend invitation sent by a registered user
    public class Invitation
    {
        // Primary key identifier for the invitation record
        [Key]
        public int InvitationId { get; set; }

        // Foreign key to the registered User who dispatched the invitation
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Recipient friend's email address
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public required string FriendEmail { get; set; }

        // Optional personalized endorsement message from the sender
        [StringLength(1000)]
        public string? Message { get; set; }

        // Timestamp when the invitation was sent
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
