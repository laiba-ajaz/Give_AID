using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    // Represents a financial donation made by a user for a specific cause
    public class Donation
    {
        // Primary key for the donation record
        [Key]
        public int DonationId { get; set; }

        // Foreign key referencing the donor User
        [Required]
        public int UserId { get; set; }

        // Navigation property for the donor User
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Foreign key referencing the supported DonationCauses
        [Required]
        public int DonationCausesID { get; set; }

        // Navigation property for the supported cause
        [ForeignKey(nameof(DonationCausesID))]
        public DonationCauses? DonationCause { get; set; }

        // Base donation amount pledged by the donor
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Indicates whether the donor elected to cover the 3% transaction fee
        public bool CoverFees { get; set; }

        // Final total amount charged (Base amount + optional 3% fee)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCharged { get; set; }

        // Giving frequency: "Once" or "Monthly"
        [StringLength(20)]
        public string Frequency { get; set; } = "Once";

        // Payment method selected by the user: "Credit" or "Debit"
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Credit";

        // Stores only the last 4 digits of the payment card for security compliance
        [StringLength(4)]
        public string CardLast4 { get; set; } = string.Empty;

        // Unique transaction identifier for payment tracking and audit
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        // Processing status: "Success", "Pending", or "Failed"
        [StringLength(20)]
        public string Status { get; set; } = "Success";

        // Timestamp when the donation was completed
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
