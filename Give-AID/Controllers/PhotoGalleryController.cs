using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;

namespace Give_AID.Controllers;

// GET: /gallery                     — programmes aur events ki saari photos
// GET: /gallery?category=Education  — sirf ek category
[Route("gallery")]
public class PhotoGalleryController : Controller
{
    private readonly AppDBContext _context;

    public PhotoGalleryController(AppDBContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(string? category)
    {
        var visible = _context.GalleryImages.AsNoTracking().Where(g => g.IsActive);

        ViewBag.Categories = visible
            .Where(g => g.Category != null && g.Category != "")
            .Select(g => g.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        ViewBag.Category = category;
        ViewBag.TotalCount = visible.Count();

        if (!string.IsNullOrWhiteSpace(category))
            visible = visible.Where(g => g.Category == category);

        var photos = visible
            .Include(g => g.Programme)
            .OrderByDescending(g => g.TakenOn ?? g.CreatedAt)
            .ThenByDescending(g => g.GalleryImageId)
            .ToList();

        return View(photos);
    }
}
