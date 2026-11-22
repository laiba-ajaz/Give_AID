using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Services;

namespace Give_AID.Controllers;

// Help Centre ke chat box ke liye chhote JSON endpoints (wwwroot/js/chat.js inhe call karta hai).
// GET  /chat/status              — admin online hai ya nahi
// POST /chat/start               — naam + email se nayi chat, token wapas
// POST /chat/send                — visitor ka message
// GET  /chat/messages?token&after — naye messages (har kuch second baad)
[Route("chat")]
public class LiveChatController : Controller
{
    private const int MaxLength = 1000;

    private readonly AppDBContext _context;
    private readonly ChatPresence _presence;

    public LiveChatController(AppDBContext context, ChatPresence presence)
    {
        _context = context;
        _presence = presence;
    }

    [HttpGet("status")]
    public IActionResult Status() => Json(new { online = _presence.IsAdminOnline });

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public IActionResult Start(string? name, string? email)
    {
        // Login user ho to us ka naam/email use karo
        if (User.Identity?.IsAuthenticated == true)
        {
            name = string.IsNullOrWhiteSpace(name) ? User.Identity.Name : name;
            email = string.IsNullOrWhiteSpace(email) ? User.FindFirstValue(ClaimTypes.Email) : email;
        }

        name = name?.Trim();
        email = email?.Trim();

        if (string.IsNullOrEmpty(name) || name.Length > 100)
            return BadRequest(new { error = "Please enter your name." });

        if (string.IsNullOrEmpty(email) || email.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            return BadRequest(new { error = "Please enter a valid email." });

        var session = new ChatSession
        {
            Token = Guid.NewGuid().ToString("N"),
            VisitorName = name,
            VisitorEmail = email
        };
        _context.ChatSessions.Add(session);
        _context.SaveChanges();

        return Json(new { token = session.Token, name = session.VisitorName });
    }

    [HttpPost("send")]
    [ValidateAntiForgeryToken]
    public IActionResult Send(string? token, string? text)
    {
        var session = FindSession(token);
        if (session == null) return NotFound(new { error = "Chat not found. Please start a new chat." });
        if (session.IsClosed) return BadRequest(new { error = "This chat has been closed. Please start a new chat." });

        text = text?.Trim();
        if (string.IsNullOrEmpty(text)) return BadRequest(new { error = "Message cannot be empty." });
        if (text.Length > MaxLength) return BadRequest(new { error = $"Message is too long (max {MaxLength} characters)." });

        var message = new ChatMessage { ChatSessionId = session.ChatSessionId, Text = text };
        _context.ChatMessages.Add(message);
        session.LastMessageAt = message.SentAt;
        _context.SaveChanges();

        return Json(ToDto(message));
    }

    [HttpGet("messages")]
    public IActionResult Messages(string? token, int after = 0)
    {
        var session = FindSession(token);
        if (session == null) return NotFound(new { error = "Chat not found." });

        var messages = _context.ChatMessages
            .Where(m => m.ChatSessionId == session.ChatSessionId && m.ChatMessageId > after)
            .OrderBy(m => m.ChatMessageId)
            .ToList()
            .Select(ToDto);

        return Json(new
        {
            closed = session.IsClosed,
            online = _presence.IsAdminOnline,
            name = session.VisitorName,
            messages
        });
    }

    private ChatSession? FindSession(string? token) =>
        string.IsNullOrWhiteSpace(token) ? null : _context.ChatSessions.FirstOrDefault(s => s.Token == token);

    private static object ToDto(ChatMessage m) => new
    {
        id = m.ChatMessageId,
        text = m.Text,
        fromAdmin = m.FromAdmin,
        time = m.SentAt.ToString("hh:mm tt")
    };
}
