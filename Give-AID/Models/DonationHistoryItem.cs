namespace Give_AID.Models
{
    // Represents a single donation record displayed on the user's donation history page
    public class DonationHistoryItem
    {
        // Unique ID of the donation
        public int DonationId { get; set; }

        // Date and time the donation occurred
        public DateTime Date { get; set; }

        // Name of the cause that received the donation
        public string CauseName { get; set; } = string.Empty;

        // Base amount donated
        public decimal Amount { get; set; }

        // Total amount charged (including fees if covered)
        public decimal TotalCharged { get; set; }

        // Frequency (One-Time or Monthly)
        public string Frequency { get; set; } = string.Empty;

        // Payment method used (Credit or Debit)
        public string PaymentMethod { get; set; } = string.Empty;

        // Masked card showing only the final four digits
        public string CardLast4 { get; set; } = string.Empty;

        // Official transaction reference ID
        public string TransactionId { get; set; } = string.Empty;

        // Status of the transaction ("Success", "Pending", etc.)
        public string Status { get; set; } = string.Empty;
    }
}
