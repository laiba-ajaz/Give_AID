using Microsoft.AspNetCore.Mvc;
using Give_AID.Areas.Admin.Models;
using Give_AID.Data;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class ContactController : Controller
{
    private readonly AppDBContext _context;

    public ContactController(AppDBContext context)
    {
        _context = context;
    }

    // ---------- Inbox ----------

    // filter: all | unread | pending (reply baqi) | replied
    public IActionResult Index(string filter = "all", string? q = null)
    {
        var search = q;
        var query = _context.ContactMessages.AsQueryable();

        switch (filter)
        {
            case "unread": query = query.Where(m => !m.IsRead); break;
            case "pending": query = query.Where(m => !m.IsReplied); break;
            case "replied": query = query.Where(m => m.IsReplied); break;
            default: filter = "all"; break;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(m => m.FullName.Contains(s) || m.Email.Contains(s) || m.Subject.Contains(s));
        }

        var model = new ContactInboxViewModel
        {
            Messages = query.OrderByDescending(m => m.CreatedAt).ToList(),
            Filter = filter,
            Search = search,
            TotalCount = _context.ContactMessages.Count(),
            UnreadCount = _context.ContactMessages.Count(m => !m.IsRead),
            PendingCount = _context.ContactMessages.Count(m => !m.IsReplied)
        };

        return View(model);
    }

    // Message kholte hi "read" ho jata hai
    public IActionResult Message(int id)
    {
        var message = _context.ContactMessages.Find(id);
        if (message == null)
        {
            TempData["Error"] = "Message not found.";
            return RedirectToAction("Index");
        }

        if (!message.IsRead)
        {
            message.IsRead = true;
            _context.SaveChanges();
        }

        return View(message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleReplied(int id)
    {
        var message = _context.ContactMessages.Find(id);
        if (message == null)
        {
            TempData["Error"] = "Message not found.";
            return RedirectToAction("Index");
        }

        message.IsReplied = !message.IsReplied;
        message.IsRead = true;
        _context.SaveChanges();

        TempData["Success"] = message.IsReplied ? "Marked as replied." : "Moved back to pending.";
        return RedirectToAction("Message", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkUnread(int id)
    {
        var message = _context.ContactMessages.Find(id);
        if (message != null)
        {
            message.IsRead = false;
            _context.SaveChanges();
            TempData["Success"] = "Marked as unread.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var message = _context.ContactMessages.Find(id);
        if (message == null)
        {
            TempData["Error"] = "Message not found.";
            return RedirectToAction("Index");
        }

        _context.ContactMessages.Remove(message);
        _context.SaveChanges();

        TempData["Success"] = $"Message from {message.FullName} deleted.";
        return RedirectToAction("Index");
    }


    // ---------- Contact details shown on the website ----------

    public IActionResult Details()
    {
        var info = _context.ContactInfos.FirstOrDefault() ?? new ContactInfo();
        return View(info);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Details(ContactInfo info)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the highlighted fields.";
            return View(info);
        }

        var existing = _context.ContactInfos.FirstOrDefault();
        if (existing == null)
        {
            existing = new ContactInfo();
            _context.ContactInfos.Add(existing);
        }

        existing.Address = info.Address;
        existing.City = info.City;
        existing.Phone = info.Phone;
        existing.Email = info.Email;
        existing.SecondEmail = info.SecondEmail;
        existing.OfficeHours = info.OfficeHours;
        existing.WeekendHours = info.WeekendHours;
        existing.MapLink = info.MapLink;
        existing.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        TempData["Success"] = "Contact details updated. They now show on the website.";
        return RedirectToAction("Details");
    }
}
