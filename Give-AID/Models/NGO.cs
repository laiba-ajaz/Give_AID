using System.ComponentModel.DataAnnotations;

public class NGO
{
    public int NGOID { get; set; }
    [StringLength(100)]
    public required string NGOName { get; set; }
    public bool isActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
