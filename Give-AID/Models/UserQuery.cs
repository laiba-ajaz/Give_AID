using System.ComponentModel.DataAnnotations;


// Help Centre ke "Raise a Direct Query" form se aayi query.
// SRS: user "Raise query" karta hai, admin "Replying queries" karta hai.
public class UserQuery
{
    public int UserQueryId { get; set; }

    // Login user ne bheji ho to us ka Id (taake "My queries" mein dikhe)
    public int? UserId { get; set; }

    [Required(ErrorMessage = "Please enter your name."), StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."), EmailAddress(ErrorMessage = "Please enter a valid email."), StringLength(100)]
    public string Email { get; set; } = "";

    [Required, StringLength(30)]
    public string Topic { get; set; } = "";

    public bool IsUrgent { get; set; }

    [Required(ErrorMessage = "Please describe your query."), StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = "";

    [StringLength(20)]
    public string Status { get; set; } = QueryStatus.Open;

    [StringLength(2000)]
    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // User ko dikhne wala number, jaise GA-00012
    public string Reference => "GA-" + UserQueryId.ToString("D5");
}

// "Track your query" page
public class QueryTrackViewModel
{
    public string? Reference { get; set; }
    public string? Email { get; set; }
    public bool Searched { get; set; }
    public UserQuery? Result { get; set; }
    public List<UserQuery> MyQueries { get; set; } = new();
}

public static class QueryStatus
{
    public const string Open = "Open";
    public const string Answered = "Answered";
    public const string Closed = "Closed";
}

// Form ke dropdown ki values (DB mein key jati hai, dikhaya naam jata hai)
public static class QueryTopics
{
    public static readonly Dictionary<string, string> All = new()
    {
        ["donation"] = "Donation & Tax Inquiries",
        ["volunteer"] = "Volunteer Verification",
        ["corporate"] = "Corporate Partnership",
        ["technical"] = "Technical Support",
        ["issue"] = "Report an Issue / Fraud Alert"
    };

    public static string Name(string key) => All.TryGetValue(key, out var name) ? name : key;
}
