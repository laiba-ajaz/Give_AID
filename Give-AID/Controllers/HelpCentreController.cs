using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Controllers;

// GET:  /HelpCentre        — FAQs, raise a query, contact us
// POST: /HelpCentre/Query  — query save (SRS: user "Raise query")
// GET:  /HelpCentre/Track  — reference number + email se query ka status aur admin ka jawab
public class HelpCentreController : Controller
{
    private readonly AppDBContext _context;

    public HelpCentreController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Query([Bind("FullName,Email,Topic,IsUrgent,Message")] UserQuery query)
    {
        if (!QueryTopics.All.ContainsKey(query.Topic ?? ""))
            ModelState.AddModelError(nameof(UserQuery.Topic), "Please choose a topic.");

        if (!ModelState.IsValid)
        {
            TempData["QueryError"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
                                     ?? "Please check the form and try again.";
            return Redirect(Url.Action("Index") + "#raise-query");
        }

        query.FullName = query.FullName.Trim();
        query.Email = query.Email.Trim();
        query.Status = QueryStatus.Open;
        query.CreatedAt = DateTime.Now;
        query.UserId = CurrentUserId();

        _context.Queries.Add(query);
        _context.SaveChanges();

        TempData["QuerySent"] = query.Reference;
        TempData["QueryEmail"] = query.Email;
        return Redirect(Url.Action("Index") + "#raise-query");
    }

    [HttpGet]
    public IActionResult Track(string? reference)
    {
        var model = new QueryTrackViewModel
        {
            Reference = reference,
            Email = User.FindFirstValue(ClaimTypes.Email),
            MyQueries = LoadMyQueries()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Track(string? reference, string? email)
    {
        var model = new QueryTrackViewModel
        {
            Reference = reference?.Trim(),
            Email = email?.Trim(),
            MyQueries = LoadMyQueries(),
            Searched = true
        };

        var id = ParseReference(model.Reference);
        if (id != null && !string.IsNullOrEmpty(model.Email))
        {
            // Email bhi match honi chahiye, warna koi bhi number badal kar doosron ki query na dekh sake
            model.Result = _context.Queries.FirstOrDefault(q => q.UserQueryId == id && q.Email == model.Email);
        }

        return View(model);
    }

    // "GA-00012", "ga00012" ya sirf "12" -> 12
    private static int? ParseReference(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return null;
        var digits = new string(reference.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var id) && id > 0 ? id : null;
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    // Login user ki apni queries (login se pehle usi email se bheji hui bhi)
    private List<UserQuery> LoadMyQueries()
    {
        if (User.Identity?.IsAuthenticated != true) return new();

        var userId = CurrentUserId();
        var email = User.FindFirstValue(ClaimTypes.Email);

        return _context.Queries
            .Where(q => (userId != null && q.UserId == userId) || (email != null && q.Email == email))
            .OrderByDescending(q => q.CreatedAt)
            .ToList();
    }
}
