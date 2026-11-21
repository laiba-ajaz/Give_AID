using System.ComponentModel.DataAnnotations;

public class NGO
{
    public int NGOID { get; set; }
    [StringLength(100)]
    public required string NGOName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    public string? LogoPath { get; set; }

    public bool isActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
