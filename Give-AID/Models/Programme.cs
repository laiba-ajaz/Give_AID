using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    public class Programme
    {
        [Key]
        public int ProgrammeId { get; set; }

        [StringLength(100)]
        public required string Title { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        public string? ImagePath { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool isActive { get; set; } = true;

        [Required]
        public int NGOID { get; set; }
        [ForeignKey(nameof(NGOID))]
        public NGO? NGO { get; set; }

        [Required]
        public int DonationCausesID { get; set; }
        [ForeignKey(nameof(DonationCausesID))]
        public DonationCauses? Cause { get; set; }
    }
}
