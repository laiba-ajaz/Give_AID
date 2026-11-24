using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Areas.Admin.Models;
using Give_AID.Data;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly AppDBContext _context;

    public DashboardController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var model = new DashboardViewModel
        {
            TotalMembers = _context.Users.Count(),
            NewMembersThisMonth = _context.Users.Count(u => u.CreatedAt >= monthStart),
            TotalCauses = _context.DonationCauses.Count(),
            ActiveCauses = _context.DonationCauses.Count(c => c.isActive),
            TotalNgos = _context.NGOs.Count(),
            ActiveNgos = _context.NGOs.Count(n => n.isActive),
            TotalProgrammes = _context.Programmes.Count(),
            UpcomingProgrammes = _context.Programmes.Count(p => p.isActive && p.StartDate >= today),

            // User queries (Help Centre se)
            TotalQueries = _context.Queries.Count(),
            OpenQueries = _context.Queries.Count(q => q.Status == QueryStatus.Open),
            RecentQueries = _context.Queries
                .AsNoTracking()
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .ToList(),

            // Programme volunteer interests
            TotalInterests = _context.ProgrammeInterests.Count(),
            RecentInterests = _context.ProgrammeInterests
                .AsNoTracking()
                .Include(pi => pi.Programme)
                .OrderByDescending(pi => pi.CreatedAt)
                .Take(5)
                .ToList(),

            RecentMembers = _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new MemberRow
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    City = u.City,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToList(),

            NextProgrammes = _context.Programmes
                .AsNoTracking()
                .Include(p => p.NGO)
                .Where(p => p.isActive && (p.StartDate >= today || (p.EndDate != null && p.EndDate >= today)))
                .OrderBy(p => p.StartDate)
                .Take(5)
                .ToList()
        };

        return View(model);
    }
}
