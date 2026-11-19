using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    // Database model representing a public or donor inquiry submitted via the Help Centre
    public class Query
    {
        // Primary key identifier for the query record
        [Key]
        public int QueryId { get; set; }

        // Optional foreign key to registered User (nullable for guest inquiries)
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Inquirer's full name
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        // Inquirer's email address for receiving updates
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public required string Email { get; set; }

        // Selected topic or category (e.g. donation, volunteer, corporate, technical, issue)
        [Required]
        [StringLength(100)]
        public required string Topic { get; set; }

        // Full inquiry message text (minimum 10 characters)
        [Required]
        [StringLength(2000)]
        public required string Message { get; set; }

        // Lifecycle status: "New" by default, updated by Admin later
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "New";

        // Admin reply text (nullable until answered by admin staff)
        public string? Reply { get; set; }

        // Timestamp when the query was submitted
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
