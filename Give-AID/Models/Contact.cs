using System.ComponentModel.DataAnnotations;


// Website par dikhne wali contact details. Table mein sirf ek row hoti hai;
// row na ho to yehi default values dikhti hain, admin pehli dafa save kare to row ban jati hai.
public class ContactInfo
{
    public int ContactInfoId { get; set; }

    [Required, StringLength(150)]
    public string Address { get; set; } = "240 Welfare Way, Suite 400";

    [StringLength(150)]
    public string? City { get; set; } = "San Francisco, CA 94107, United States";

    [Required, StringLength(30)]
    public string Phone { get; set; } = "+1 (800) 448-3243";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "support@give-aid.org";

    [EmailAddress, StringLength(100)]
    public string? SecondEmail { get; set; } = "partnerships@give-aid.org";

    [StringLength(100)]
    public string? OfficeHours { get; set; } = "Monday – Friday: 8:00 AM – 8:00 PM";

    [StringLength(100)]
    public string? WeekendHours { get; set; } = "Saturday: 9:00 AM – 4:00 PM · Sunday: Closed";

    [Url, StringLength(300)]
    public string? MapLink { get; set; } = "https://maps.google.com/?q=240+Welfare+Way+San+Francisco+CA+94107";

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}


// Contact Us form se aaya hua message
public class ContactMessage
{
    public int ContactMessageId { get; set; }

    [Required(ErrorMessage = "Please enter your name."), StringLength(100)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."), EmailAddress(ErrorMessage = "Please enter a valid email."), StringLength(100)]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "Please enter a valid phone number."), StringLength(30)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please enter a subject."), StringLength(150)]
    public string Subject { get; set; } = "";

    [Required(ErrorMessage = "Please write your message."), StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = "";

    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
