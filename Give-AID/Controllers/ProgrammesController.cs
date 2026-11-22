using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;

namespace Give_AID.Controllers;

// GET: /Programmes            — admin panel ke programmes + "Register your interest" form
// GET: /Programmes?select=5   — home page se aaye to wahi programme pehle se chuna hua
public class ProgrammesController : Controller
{
    private readonly AppDBContext _context;

    public ProgrammesController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? select)
    {
        var today = DateTime.Today;

        var all = _context.Programmes
            .AsNoTracking()
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .Where(p => p.isActive)
            .ToList();

        // Aane wale (aur jo chal rahe hain) pehle, qareeb wali date upar; khatam ho chuke baad mein, naye pehle
        var upcoming = all.Where(p => !IsFinished(p, today)).OrderBy(p => p.StartDate);
        var finished = all.Where(p => IsFinished(p, today)).OrderByDescending(p => p.StartDate);
        var programmes = upcoming.Concat(finished).ToList();

        ViewBag.Selected = programmes.FirstOrDefault(p => p.ProgrammeId == select) ?? programmes.FirstOrDefault(p => !IsFinished(p, today)) ?? programmes.FirstOrDefault();
        ViewBag.NgoCount = _context.NGOs.Count(n => n.isActive);

        return View(programmes);
    }

    public static bool IsFinished(Give_AID.Models.Programme p, DateTime today) => (p.EndDate ?? p.StartDate) < today;
}
