using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class AboutController : Controller
{
    private const string ImageFolder = "about";

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public AboutController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index()
    {
        var pages = _context.AboutPages
            .Include(p => p.Items)
            .OrderBy(p => p.SortOrder)
            .ToList();
        return View(pages);
    }

    public IActionResult Edit(int id)
    {
        var page = _context.AboutPages
            .Include(p => p.Items.OrderBy(i => i.SortOrder).ThenBy(i => i.AboutItemId))
            .FirstOrDefault(p => p.AboutPageId == id);

        if (page == null)
        {
            TempData["Error"] = "Page not found.";
            return RedirectToAction("Index");
        }

        return View(page);
    }


    // ---------- Page text & image ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePage(AboutPage page, IFormFile? ImageFile, bool RemoveImage = false)
    {
        var existing = _context.AboutPages.Find(page.AboutPageId);
        if (existing == null)
        {
            TempData["Error"] = "Page not found.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Edit", new { id = existing.AboutPageId });
        }

        if (ImageFile != null)
        {
            string newPath;
            try { newPath = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Edit", new { id = existing.AboutPageId });
            }
            _images.Delete(existing.ImagePath);
            existing.ImagePath = newPath;
        }
        else if (RemoveImage)
        {
            _images.Delete(existing.ImagePath);
            existing.ImagePath = null;
        }

        existing.Title = page.Title;
        existing.Heading = page.Heading;
        existing.Content = page.Content;
        existing.IsActive = page.IsActive;
        existing.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        TempData["Success"] = $"\"{existing.Title}\" page updated.";
        return RedirectToAction("Edit", new { id = existing.AboutPageId });
    }


    // ---------- Cards on the page (team members, jobs, achievements ...) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(AboutItem item, IFormFile? ImageFile)
    {
        if (!_context.AboutPages.Any(p => p.AboutPageId == item.AboutPageId))
        {
            TempData["Error"] = "Page not found.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Edit", new { id = item.AboutPageId });
        }

        item.ImagePath = null;
        if (ImageFile != null)
        {
            try { item.ImagePath = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Edit", new { id = item.AboutPageId });
            }
        }

        _context.AboutItems.Add(item);
        _context.SaveChanges();
        TempData["Success"] = $"\"{item.Title}\" added.";
        return RedirectToAction("Edit", new { id = item.AboutPageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(AboutItem item, IFormFile? ImageFile, bool RemoveImage = false)
    {
        var existing = _context.AboutItems.Find(item.AboutItemId);
        if (existing == null)
        {
            TempData["Error"] = "Item not found.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Edit", new { id = existing.AboutPageId });
        }

        if (ImageFile != null)
        {
            string newPath;
            try { newPath = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Edit", new { id = existing.AboutPageId });
            }
            _images.Delete(existing.ImagePath);
            existing.ImagePath = newPath;
        }
        else if (RemoveImage)
        {
            _images.Delete(existing.ImagePath);
            existing.ImagePath = null;
        }

        existing.Title = item.Title;
        existing.Subtitle = item.Subtitle;
        existing.Description = item.Description;
        existing.SortOrder = item.SortOrder;
        existing.IsActive = item.IsActive;

        _context.SaveChanges();
        TempData["Success"] = $"\"{existing.Title}\" updated.";
        return RedirectToAction("Edit", new { id = existing.AboutPageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteItem(int id)
    {
        var item = _context.AboutItems.Find(id);
        if (item == null)
        {
            TempData["Error"] = "Item not found.";
            return RedirectToAction("Index");
        }

        _context.AboutItems.Remove(item);
        _context.SaveChanges();
        _images.Delete(item.ImagePath);

        TempData["Success"] = $"\"{item.Title}\" deleted.";
        return RedirectToAction("Edit", new { id = item.AboutPageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleItem(int id)
    {
        var item = _context.AboutItems.Find(id);
        if (item == null)
        {
            TempData["Error"] = "Item not found.";
            return RedirectToAction("Index");
        }

        item.IsActive = !item.IsActive;
        _context.SaveChanges();

        TempData["Success"] = item.IsActive ? "Shown on website." : "Hidden from website.";
        return RedirectToAction("Edit", new { id = item.AboutPageId });
    }
}
