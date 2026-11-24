using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Areas.Admin.Controllers;

// =========================================================================
// PROGRAMME INTERESTS (Admin Panel)
// Welfare programmes par user ne jo "Show Interest" form submit kiya hai
// (Volunteer, Donor, etc.), un tamam registrations ki list yahan aati hai.
// =========================================================================
[Area("Admin")]
public class ProgrammeInterestsController : Controller
{
    private readonly AppDBContext _context;

    public ProgrammeInterestsController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Admin/ProgrammeInterests - Tamam volunteer aur participant registrations dikhata hai
    public async Task<IActionResult> Index(int? programmeId, string? q)
    {
        var query = _context.ProgrammeInterests
            .AsNoTracking()
            .Include(pi => pi.Programme)
            .AsQueryable();

        // Specific programme filter
        if (programmeId.HasValue && programmeId.Value > 0)
        {
            query = query.Where(pi => pi.ProgrammeId == programmeId.Value);
        }

        // Search by name, email or phone
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(pi => pi.Name.Contains(s) || pi.Email.Contains(s) || (pi.Phone != null && pi.Phone.Contains(s)));
        }

        var list = await query.OrderByDescending(pi => pi.CreatedAt).ToListAsync();

        ViewBag.Programmes = await _context.Programmes.Where(p => p.isActive).OrderBy(p => p.Title).ToListAsync();
        ViewBag.SelectedProgramme = programmeId;
        ViewBag.Search = q;
        ViewBag.TotalCount = await _context.ProgrammeInterests.CountAsync();

        return View(list);
    }

    // POST: /Admin/ProgrammeInterests/Delete/5 - Entry delete karne ke liye
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ProgrammeInterests.FindAsync(id);
        if (item != null)
        {
            _context.ProgrammeInterests.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Volunteer registration deleted.";
        }
        return RedirectToAction("Index");
    }
}
