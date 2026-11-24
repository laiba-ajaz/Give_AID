using System.ComponentModel.DataAnnotations;


// Help Centre ki "Launch Chat" se shuru hone wali ek baat-cheet
public class ChatSession
{
    public int ChatSessionId { get; set; }

    // Visitor ke browser mein save hota hai, isi se pata chalta hai kaunsi chat uski hai
    [Required, StringLength(40)]
    public string Token { get; set; } = "";

    [Required, StringLength(100)]
    public string VisitorName { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string VisitorEmail { get; set; } = "";

    public bool IsClosed { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime LastMessageAt { get; set; } = DateTime.Now;

    public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int ChatSessionId { get; set; }
    public ChatSession? Session { get; set; }

    [Required, StringLength(1000)]
    public string Text { get; set; } = "";

    public bool FromAdmin { get; set; }

    // Admin ne visitor ka message parh liya (sidebar ki ginti ke liye)
    public bool IsReadByAdmin { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;
}
