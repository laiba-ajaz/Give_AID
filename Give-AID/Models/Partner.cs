using System.ComponentModel.DataAnnotations;


public class Partner
{
    public int PartnerId { get; set; }

    [Required, StringLength(100)]
    public string PartnerName { get; set; } = "";

    public string? LogoPath { get; set; }

    [Url, StringLength(200)]
    public string? WebsiteUrl { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
