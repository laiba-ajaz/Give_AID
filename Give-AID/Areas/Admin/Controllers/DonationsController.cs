using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Areas.Admin.Controllers;

// =========================================================================
// DONATIONS (Admin Panel)
// Website ke Donate page se aane wali tamam donations ki list aur stats yahan aate hain.
// =========================================================================
[Area("Admin")]
public class DonationsController : Controller
{
    private readonly AppDBContext _context;

    public DonationsController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Admin/Donations - Tamam financial contributions aur stats dikhata hai
    public async Task<IActionResult> Index(int? causeId, string? q)
    {
        var query = _context.Donations
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.DonationCause)
            .AsQueryable();

        // Cause filter
        if (causeId.HasValue && causeId.Value > 0)
        {
            query = query.Where(d => d.DonationCausesID == causeId.Value);
        }

        // Search by donor name, email or transaction ID
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(d =>
                (d.User != null && (d.User.FullName.Contains(s) || d.User.Email.Contains(s))) ||
                d.TransactionId.Contains(s));
        }

        var donations = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        // Summary calculations
        decimal totalRaised = await _context.Donations.SumAsync(d => (decimal?)d.TotalCharged) ?? 0m;
        int totalCount = await _context.Donations.CountAsync();
        decimal avgDonation = totalCount > 0 ? Math.Round(totalRaised / totalCount, 2) : 0m;

        ViewBag.TotalRaised = totalRaised;
        ViewBag.TotalCount = totalCount;
        ViewBag.AvgDonation = avgDonation;
        ViewBag.Causes = await _context.DonationCauses.Where(c => c.isActive).OrderBy(c => c.CauseName).ToListAsync();
        ViewBag.SelectedCause = causeId;
        ViewBag.Search = q;

        return View(donations);
    }
}
