using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class NGOController : Controller
{
    private const string ImageFolder = "ngos";

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public NGOController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index()
    {
        var ngosName = _context.NGOs.ToList();
        return View(ngosName);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNgo(NGO nGO, IFormFile? LogoFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        nGO.LogoPath = null;
        if (LogoFile != null)
        {
            try { nGO.LogoPath = await _images.SaveAsync(LogoFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        _context.NGOs.Add(nGO);
        _context.SaveChanges();
        TempData["Success"] = "NGO added successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(NGO nGO, IFormFile? LogoFile, bool RemoveImage = false)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        var existing = _context.NGOs.Find(nGO.NGOID);
        if (existing == null)
        {
            TempData["Error"] = "NGO not found.";
            return RedirectToAction("Index");
        }

        if (LogoFile != null)
        {
            string newPath;
            try { newPath = await _images.SaveAsync(LogoFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            _images.Delete(existing.LogoPath);
            existing.LogoPath = newPath;
        }
        else if (RemoveImage)
        {
            _images.Delete(existing.LogoPath);
            existing.LogoPath = null;
        }

        existing.NGOName = nGO.NGOName;
        existing.Description = nGO.Description;
        existing.Website = nGO.Website;
        existing.isActive = nGO.isActive;

        _context.SaveChanges();
        TempData["Success"] = "NGO updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DltNGO(int id)
    {
        var ngo = _context.NGOs.Find(id);
        if (ngo == null)
        {
            TempData["Error"] = "NGO not found.";
            return RedirectToAction("Index");
        }

        if (_context.Programmes.Any(p => p.NGOID == id))
        {
            TempData["Error"] = "This NGO has programmes. Delete them first or deactivate the NGO.";
            return RedirectToAction("Index");
        }

        _context.NGOs.Remove(ngo);
        _context.SaveChanges();
        _images.Delete(ngo.LogoPath);
        TempData["Success"] = "NGO deleted successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var ngo = _context.NGOs.Find(id);
        if (ngo == null)
        {
            TempData["Error"] = "NGO not found.";
            return RedirectToAction("Index");
        }

        ngo.isActive = !ngo.isActive;
        _context.SaveChanges();

        TempData["Success"] = ngo.isActive ? "NGO activated." : "NGO deactivated.";
        return RedirectToAction("Index");
    }


}
