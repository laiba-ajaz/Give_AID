using System.ComponentModel.DataAnnotations;
using Give_AID.Models;


// Gallery ki ek photo. SRS: "Images of different programmes conducted must be added by the admin".
// Programme optional hai (photo kisi general event ki bhi ho sakti hai), Category filter ke liye hai.
public class GalleryImage
{
    public int GalleryImageId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(250)]
    public string? Caption { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public int? ProgrammeId { get; set; }
    public Programme? Programme { get; set; }

    public string ImagePath { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime? TakenOn { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
