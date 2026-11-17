using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class ProgrammesController : Controller
{
    private const string ImageFolder = "programmes";

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public ProgrammesController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index()
    {
        var programmes = _context.Programmes
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .OrderByDescending(p => p.StartDate)
            .ToList();

        ViewBag.NGOs = _context.NGOs.Where(n => n.isActive).OrderBy(n => n.NGOName).ToList();
        ViewBag.Causes = _context.DonationCauses.Where(c => c.isActive).OrderBy(c => c.CauseName).ToList();

        return View(programmes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProgramme(Programme programme, IFormFile? ImageFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        if (programme.EndDate != null && programme.EndDate < programme.StartDate)
        {
            TempData["Error"] = "End date cannot be before start date.";
            return RedirectToAction("Index");
        }

        programme.ImagePath = null;
        if (ImageFile != null)
        {
            try { programme.ImagePath = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        _context.Programmes.Add(programme);
        _context.SaveChanges();
        TempData["Success"] = "Programme added successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProgramme(Programme programme, IFormFile? ImageFile, bool RemoveImage = false)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        if (programme.EndDate != null && programme.EndDate < programme.StartDate)
        {
            TempData["Error"] = "End date cannot be before start date.";
            return RedirectToAction("Index");
        }

        var existing = _context.Programmes.Find(programme.ProgrammeId);
        if (existing == null)
        {
            TempData["Error"] = "Programme not found.";
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
        else if (RemoveImage)
        {
            _images.Delete(existing.ImagePath);
            existing.ImagePath = null;
        }

        existing.Title = programme.Title;
        existing.Description = programme.Description;
        existing.StartDate = programme.StartDate;
        existing.EndDate = programme.EndDate;
        existing.Location = programme.Location;
        existing.isActive = programme.isActive;
        existing.NGOID = programme.NGOID;
        existing.DonationCausesID = programme.DonationCausesID;

        _context.SaveChanges();
        TempData["Success"] = "Programme updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DltProgramme(int id)
    {
        var programme = _context.Programmes.Find(id);
        if (programme == null)
        {
            TempData["Error"] = "Programme not found.";
            return RedirectToAction("Index");
        }

        _context.Programmes.Remove(programme);
        _context.SaveChanges();
        _images.Delete(programme.ImagePath);
        TempData["Success"] = "Programme deleted successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var programme = _context.Programmes.Find(id);
        if (programme == null)
        {
            TempData["Error"] = "Programme not found.";
            return RedirectToAction("Index");
        }

        programme.isActive = !programme.isActive;
        _context.SaveChanges();

        TempData["Success"] = programme.isActive ? "Programme activated." : "Programme deactivated.";
        return RedirectToAction("Index");
    }
}
