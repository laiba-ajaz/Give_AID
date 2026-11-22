using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;

namespace Give_AID.Controllers;

// GET: /about-us            — poora About Us ek page par (saare sections)
// GET: /about-us/our-team   — koi ek sub-page uske slug se
[Route("about-us")]
public class AboutUsController : Controller
{
    private readonly AppDBContext _context;

    public AboutUsController(AppDBContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var pages = _context.AboutPages
            .AsNoTracking()
            .Include(p => p.Items.Where(i => i.IsActive).OrderBy(i => i.SortOrder).ThenBy(i => i.AboutItemId))
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToList();

        if (pages.Count == 0) return RedirectToAction("Index", "Home");

        return View(pages);
    }

    [HttpGet("{slug}")]
    public IActionResult Section(string slug) => ShowPage(slug);

    private IActionResult ShowPage(string slug)
    {
        var page = _context.AboutPages
            .AsNoTracking()
            .Include(p => p.Items.Where(i => i.IsActive).OrderBy(i => i.SortOrder).ThenBy(i => i.AboutItemId))
            .FirstOrDefault(p => p.Slug == slug && p.IsActive);

        if (page == null) return NotFound();

        // Upar wale tabs ke liye baqi pages
        ViewBag.Menu = _context.AboutPages
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToList();

        return View("Section", page);
    }
}
