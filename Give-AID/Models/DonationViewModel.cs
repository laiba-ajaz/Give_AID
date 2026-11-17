using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // ViewModel representing the donation form submission and view data
    public class DonationViewModel
    {
        // Selected cause identifier or key (e.g., cause ID as string or key)
        public string? Cause { get; set; }

        // Pledged base donation amount
        [Range(1, 1000000, ErrorMessage = "Donation amount must be at least $1.00.")]
        public decimal Amount { get; set; } = 50.00m;

        // Payment frequency ("OneTime" or "Monthly")
        public string Frequency { get; set; } = "OneTime";

        // Whether user opts to cover 3% transaction fee
        public bool CoverFees { get; set; } = true;

        // Calculated total amount to be charged
        public decimal TotalAmount { get; set; } = 51.50m;

        // Payment card type ("Credit" or "Debit")
        public string CardType { get; set; } = "Credit";

        // Cardholder full name
        [Required(ErrorMessage = "Name on card is required.")]
        public string? CardHolderName { get; set; }

        // Full card number (validated via Luhn algorithm on server, never stored in DB)
        [Required(ErrorMessage = "Card number is required.")]
        public string? CardNumber { get; set; }

        // Card expiration date in MM/YY format
        [Required(ErrorMessage = "Expiration date is required (MM/YY).")]
        public string? ExpiryDate { get; set; }

        // Security code (CVV/CVC, 3-4 digits, never stored in DB)
        [Required(ErrorMessage = "Security code (CVV) is required.")]
        public string? Cvv { get; set; }

        // List of active causes fetched from the database for the cause selector
        public List<DonationCauses> ActiveCauses { get; set; } = new();

        // Flag indicating if a donation was completed successfully (to trigger the modal)
        public bool IsSuccess { get; set; }

        // Generated transaction ID returned after successful payment
        public string? TransactionId { get; set; }

        // Name of the cause for receipt modal
        public string? ConfirmedCauseName { get; set; }

        // Donor display name for receipt modal
        public string? DonorName { get; set; }

        // Final charged amount for receipt modal
        public decimal ConfirmedTotalAmount { get; set; }
    }
}
