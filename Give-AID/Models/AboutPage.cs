using System.ComponentModel.DataAnnotations;


// About Us ka ek sub-page (What we do, Our mission, Our team ...).
// Slug fix hai, baqi sab admin panel se edit hota hai.
public class AboutPage
{
    public int AboutPageId { get; set; }

    [Required, StringLength(50)]
    public string Slug { get; set; } = "";

    [Required, StringLength(80)]
    public string Title { get; set; } = "";

    [StringLength(150)]
    public string? Heading { get; set; }

    [StringLength(4000)]
    public string? Content { get; set; }

    public string? ImagePath { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<AboutItem> Items { get; set; } = new();
}


// Page ke andar wale cards: team member, job opening, achievement, supporter etc.
public class AboutItem
{
    public int AboutItemId { get; set; }

    public int AboutPageId { get; set; }
    public AboutPage? Page { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(100)]
    public string? Subtitle { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
