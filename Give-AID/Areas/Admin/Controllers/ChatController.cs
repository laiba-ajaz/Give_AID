using Microsoft.AspNetCore.Mvc;
using Give_AID.Areas.Admin.Models;
using Give_AID.Data;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class ChatController : Controller
{
    private const int MaxLength = 1000;

    private readonly AppDBContext _context;
    private readonly ChatPresence _presence;

    public ChatController(AppDBContext context, ChatPresence presence)
    {
        _context = context;
        _presence = presence;
    }

    // Bayein taraf saari chats, daayein taraf chuni hui chat
    public IActionResult Index(int? id)
    {
        _presence.AdminSeen();

        var sessions = LoadSessions();
        var selectedId = id ?? sessions.FirstOrDefault()?.Id;

        ChatSession? selected = null;
        var messages = new List<ChatMessage>();

        if (selectedId != null)
        {
            selected = _context.ChatSessions.Find(selectedId.Value);
            if (selected != null)
            {
                messages = _context.ChatMessages
                    .Where(m => m.ChatSessionId == selected.ChatSessionId)
                    .OrderBy(m => m.ChatMessageId)
                    .ToList();

                MarkRead(messages);

                // Abhi parhi gayi chat ki ginti list mein bhi zero dikhe
                var row = sessions.FirstOrDefault(s => s.Id == selected.ChatSessionId);
                if (row != null) row.Unread = 0;
            }
        }

        return View(new ChatInboxViewModel
        {
            Sessions = sessions,
            Selected = selected,
            Messages = messages
        });
    }

    // Admin ka page har kuch second baad yeh call karta hai: naye messages + list ki update
    public IActionResult Poll(int id, int after = 0)
    {
        _presence.AdminSeen();

        var messages = _context.ChatMessages
            .Where(m => m.ChatSessionId == id && m.ChatMessageId > after)
            .OrderBy(m => m.ChatMessageId)
            .ToList();

        MarkRead(messages);

        var sessions = LoadSessions();
        var current = sessions.FirstOrDefault(s => s.Id == id);
        if (current != null) current.Unread = 0;

        return Json(new
        {
            messages = messages.Select(ToDto),
            sessions = sessions.Select(s => new
            {
                id = s.Id,
                name = s.VisitorName,
                lastText = s.LastText,
                lastTime = s.LastMessageAt.ToString("dd MMM, hh:mm tt"),
                unread = s.Unread,
                closed = s.IsClosed
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reply(int id, string? text)
    {
        var session = _context.ChatSessions.Find(id);
        if (session == null) return NotFound(new { error = "Chat not found." });
        if (session.IsClosed) return BadRequest(new { error = "This chat is closed. Reopen it to reply." });

        text = text?.Trim();
        if (string.IsNullOrEmpty(text)) return BadRequest(new { error = "Reply cannot be empty." });
        if (text.Length > MaxLength) return BadRequest(new { error = $"Reply is too long (max {MaxLength} characters)." });

        var message = new ChatMessage
        {
            ChatSessionId = id,
            Text = text,
            FromAdmin = true,
            IsReadByAdmin = true
        };
        _context.ChatMessages.Add(message);
        session.LastMessageAt = message.SentAt;
        _context.SaveChanges();

        _presence.AdminSeen();
        return Json(ToDto(message));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleClosed(int id)
    {
        var session = _context.ChatSessions.Find(id);
        if (session == null)
        {
            TempData["Error"] = "Chat not found.";
            return RedirectToAction("Index");
        }

        session.IsClosed = !session.IsClosed;
        _context.SaveChanges();

        TempData["Success"] = session.IsClosed ? "Chat closed. The visitor can start a new one." : "Chat reopened.";
        return RedirectToAction("Index", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var session = _context.ChatSessions.Find(id);
        if (session == null)
        {
            TempData["Error"] = "Chat not found.";
            return RedirectToAction("Index");
        }

        _context.ChatSessions.Remove(session);
        _context.SaveChanges();

        TempData["Success"] = $"Chat with {session.VisitorName} deleted.";
        return RedirectToAction("Index");
    }

    // Khuli chats pehle, phir jis mein aakhri message sab se naya ho
    private List<ChatSessionRow> LoadSessions() =>
        _context.ChatSessions
            .OrderBy(s => s.IsClosed)
            .ThenByDescending(s => s.LastMessageAt)
            .Select(s => new ChatSessionRow
            {
                Id = s.ChatSessionId,
                VisitorName = s.VisitorName,
                VisitorEmail = s.VisitorEmail,
                IsClosed = s.IsClosed,
                LastMessageAt = s.LastMessageAt,
                LastText = s.Messages.OrderByDescending(m => m.ChatMessageId).Select(m => m.Text).FirstOrDefault(),
                Unread = s.Messages.Count(m => !m.FromAdmin && !m.IsReadByAdmin)
            })
            .ToList();

    private void MarkRead(List<ChatMessage> messages)
    {
        var unread = messages.Where(m => !m.FromAdmin && !m.IsReadByAdmin).ToList();
        if (unread.Count == 0) return;

        unread.ForEach(m => m.IsReadByAdmin = true);
        _context.SaveChanges();
    }

    private static object ToDto(ChatMessage m) => new
    {
        id = m.ChatMessageId,
        text = m.Text,
        fromAdmin = m.FromAdmin,
        time = m.SentAt.ToString("hh:mm tt")
    };
}
