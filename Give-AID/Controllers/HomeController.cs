using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Controllers;

public class HomeController : Controller
{
    private readonly AppDBContext _context;

    public HomeController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var today = DateTime.Today;

        // Admin panel se add hue causes (Choose a cause section)
        ViewBag.Causes = _context.DonationCauses
            .AsNoTracking()
            .Where(c => c.isActive)
            .OrderBy(c => c.CauseName)
            .Take(6)
            .ToList();

        // Aane wale programmes (jo chal rahe hain woh bhi)
        var upcoming = _context.Programmes
            .AsNoTracking()
            .Where(p => p.isActive && (p.StartDate >= today || (p.EndDate != null && p.EndDate >= today)));
        ViewBag.UpcomingCount = upcoming.Count();
        ViewBag.UpcomingProgrammes = upcoming
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .OrderBy(p => p.StartDate)
            .Take(3)
            .ToList();

        ViewBag.Partners = _context.Partners
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartnerName)
            .ToList();

        // Home page ki photo strip ke liye gallery ki 5 nayi photos
        ViewBag.GalleryPhotos = _context.GalleryImages
            .AsNoTracking()
            .Include(g => g.Programme)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.TakenOn ?? g.CreatedAt)
            .ThenByDescending(g => g.GalleryImageId)
            .Take(5)
            .ToList();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
