using Microsoft.AspNetCore.Mvc;
using Give_AID.Areas.Admin.Models;
using Give_AID.Data;

namespace Give_AID.Areas.Admin.Controllers;

// SRS: admin "Replying queries" — Help Centre se aayi queries ka jawab yahan se jata hai
[Area("Admin")]
public class QueriesController : Controller
{
    private readonly AppDBContext _context;

    public QueriesController(AppDBContext context)
    {
        _context = context;
    }

    // filter: all | open | urgent | answered | closed
    public IActionResult Index(string filter = "open", string? q = null)
    {
        var query = _context.Queries.AsQueryable();

        switch (filter)
        {
            case "open": query = query.Where(x => x.Status == QueryStatus.Open); break;
            case "urgent": query = query.Where(x => x.IsUrgent && x.Status == QueryStatus.Open); break;
            case "answered": query = query.Where(x => x.Status == QueryStatus.Answered); break;
            case "closed": query = query.Where(x => x.Status == QueryStatus.Closed); break;
            default: filter = "all"; break;
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            // "GA-00012" likh kar bhi dhoond sakein
            var digits = new string(s.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out var id);
            query = query.Where(x => x.FullName.Contains(s) || x.Email.Contains(s) || x.Message.Contains(s) || (id > 0 && x.UserQueryId == id));
        }

        var model = new QueryInboxViewModel
        {
            // Urgent pehle, phir purani pehle (jo sab se zyada der se intezar mein hai)
            Queries = query.OrderByDescending(x => x.Status == QueryStatus.Open && x.IsUrgent)
                           .ThenBy(x => x.Status == QueryStatus.Open ? 0 : 1)
                           .ThenBy(x => x.Status == QueryStatus.Open ? x.CreatedAt : DateTime.MaxValue)
                           .ThenByDescending(x => x.CreatedAt)
                           .ToList(),
            Filter = filter,
            Search = q,
            TotalCount = _context.Queries.Count(),
            OpenCount = _context.Queries.Count(x => x.Status == QueryStatus.Open),
            UrgentCount = _context.Queries.Count(x => x.Status == QueryStatus.Open && x.IsUrgent),
            AnsweredCount = _context.Queries.Count(x => x.Status == QueryStatus.Answered),
            ClosedCount = _context.Queries.Count(x => x.Status == QueryStatus.Closed)
        };

        return View(model);
    }

    public IActionResult Details(int id)
    {
        var query = _context.Queries.Find(id);
        if (query == null)
        {
            TempData["Error"] = "Query not found.";
            return RedirectToAction("Index");
        }
        return View(query);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reply(int id, string? reply)
    {
        var query = _context.Queries.Find(id);
        if (query == null)
        {
            TempData["Error"] = "Query not found.";
            return RedirectToAction("Index");
        }

        reply = reply?.Trim();
        if (string.IsNullOrEmpty(reply) || reply.Length < 5)
        {
            TempData["Error"] = "Please write a reply (at least 5 characters).";
            return RedirectToAction("Details", new { id });
        }
        if (reply.Length > 2000)
        {
            TempData["Error"] = "Reply is too long (max 2000 characters).";
            return RedirectToAction("Details", new { id });
        }

        var firstReply = string.IsNullOrEmpty(query.AdminReply);
        query.AdminReply = reply;
        query.RepliedAt = DateTime.Now;
        query.Status = QueryStatus.Answered;
        _context.SaveChanges();

        TempData["Success"] = firstReply
            ? $"Reply sent. {query.FullName} can see it on the Track Query page with {query.Reference}."
            : "Reply updated.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetStatus(int id, string status)
    {
        var query = _context.Queries.Find(id);
        if (query == null)
        {
            TempData["Error"] = "Query not found.";
            return RedirectToAction("Index");
        }

        if (status != QueryStatus.Open && status != QueryStatus.Closed && status != QueryStatus.Answered)
        {
            TempData["Error"] = "Unknown status.";
            return RedirectToAction("Details", new { id });
        }

        // Jawab ke bagair "Answered" nahi ho sakta
        if (status == QueryStatus.Answered && string.IsNullOrEmpty(query.AdminReply))
            status = QueryStatus.Open;

        query.Status = status;
        _context.SaveChanges();

        TempData["Success"] = status == QueryStatus.Closed ? "Query closed." : "Query reopened.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var query = _context.Queries.Find(id);
        if (query == null)
        {
            TempData["Error"] = "Query not found.";
            return RedirectToAction("Index");
        }

        _context.Queries.Remove(query);
        _context.SaveChanges();

        TempData["Success"] = $"Query {query.Reference} deleted.";
        return RedirectToAction("Index");
    }
}
