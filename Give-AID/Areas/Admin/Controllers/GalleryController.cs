using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class GalleryController : Controller
{
    private const string ImageFolder = "gallery";
    private const int MaxFilesPerUpload = 20;

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public GalleryController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index(string? category)
    {
        var query = _context.GalleryImages.Include(g => g.Programme).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(g => g.Category == category);

        var photos = query
            .OrderByDescending(g => g.TakenOn ?? g.CreatedAt)
            .ThenByDescending(g => g.GalleryImageId)
            .ToList();

        var usedCategories = _context.GalleryImages
            .Where(g => g.Category != null && g.Category != "")
            .Select(g => g.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ViewBag.Category = category;
        ViewBag.Categories = usedCategories;
        ViewBag.TotalCount = _context.GalleryImages.Count();
        ViewBag.Programmes = _context.Programmes.Include(p => p.Cause).OrderByDescending(p => p.StartDate).ToList();

        // Category likhte waqt suggestions: pehle se use hui categories + donation causes ke naam
        ViewBag.Suggestions = usedCategories
            .Union(_context.DonationCauses.Select(c => c.CauseName).ToList())
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return View(photos);
    }

    // Ek saath kai photos upload; title/caption/category sab photos par lagti hai
    // 20 photos x 2 MB = 40 MB; default limit (~30 MB) is liye barhai hai
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
    public async Task<IActionResult> Upload(List<IFormFile> ImageFiles, string? Title, string? Caption,
                                            string? Category, int? ProgrammeId, DateTime? TakenOn)
    {
        if (ImageFiles == null || ImageFiles.Count == 0)
        {
            TempData["Error"] = "Please choose at least one photo.";
            return RedirectToAction("Index");
        }

        if (ImageFiles.Count > MaxFilesPerUpload)
        {
            TempData["Error"] = $"You can upload up to {MaxFilesPerUpload} photos at a time.";
            return RedirectToAction("Index");
        }

        if (ProgrammeId != null && !_context.Programmes.Any(p => p.ProgrammeId == ProgrammeId))
            ProgrammeId = null;

        int saved = 0;
        var failed = new List<string>();

        foreach (var file in ImageFiles)
        {
            string path;
            try { path = await _images.SaveAsync(file, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                failed.Add($"{file.FileName}: {ex.Message}");
                continue;
            }

            _context.GalleryImages.Add(new GalleryImage
            {
                Title = Limit(string.IsNullOrWhiteSpace(Title) ? TitleFromFileName(file.FileName) : Title.Trim(), 100),
                Caption = string.IsNullOrWhiteSpace(Caption) ? null : Limit(Caption.Trim(), 250),
                Category = string.IsNullOrWhiteSpace(Category) ? null : Limit(Category.Trim(), 50),
                ProgrammeId = ProgrammeId,
                TakenOn = TakenOn,
                ImagePath = path
            });
            saved++;
        }

        _context.SaveChanges();

        if (saved > 0)
            TempData["Success"] = saved == 1 ? "1 photo added to the gallery." : $"{saved} photos added to the gallery.";
        if (failed.Count > 0)
            TempData["Error"] = "Some photos were not uploaded – " + string.Join(" · ", failed);

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GalleryImage photo, IFormFile? ImageFile)
    {
        // ImagePath form se nahi aata, is liye us ki validation hata do
        ModelState.Remove(nameof(GalleryImage.ImagePath));

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        var existing = _context.GalleryImages.Find(photo.GalleryImageId);
        if (existing == null)
        {
            TempData["Error"] = "Photo not found.";
            return RedirectToAction("Index");
        }

        if (ImageFile != null)
        {
            string newPath;
            try { newPath = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            _images.Delete(existing.ImagePath);
            existing.ImagePath = newPath;
        }

        if (photo.ProgrammeId != null && !_context.Programmes.Any(p => p.ProgrammeId == photo.ProgrammeId))
            photo.ProgrammeId = null;

        existing.Title = photo.Title.Trim();
        existing.Caption = photo.Caption?.Trim();
        existing.Category = string.IsNullOrWhiteSpace(photo.Category) ? null : photo.Category.Trim();
        existing.ProgrammeId = photo.ProgrammeId;
        existing.TakenOn = photo.TakenOn;
        existing.IsActive = photo.IsActive;

        _context.SaveChanges();
        TempData["Success"] = "Photo updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var photo = _context.GalleryImages.Find(id);
        if (photo == null)
        {
            TempData["Error"] = "Photo not found.";
            return RedirectToAction("Index");
        }

        _context.GalleryImages.Remove(photo);
        _context.SaveChanges();
        _images.Delete(photo.ImagePath);

        TempData["Success"] = $"\"{photo.Title}\" deleted.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var photo = _context.GalleryImages.Find(id);
        if (photo == null)
        {
            TempData["Error"] = "Photo not found.";
            return RedirectToAction("Index");
        }

        photo.IsActive = !photo.IsActive;
        _context.SaveChanges();

        TempData["Success"] = photo.IsActive ? "Photo shown on website." : "Photo hidden from website.";
        return RedirectToAction("Index");
    }

    // "school-drive_2024.jpg" -> "School drive 2024"
    private static string TitleFromFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ').Replace('_', ' ').Trim();
        if (name.Length == 0) return "Untitled photo";
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private static string Limit(string value, int max) => value.Length <= max ? value : value.Substring(0, max);
}
