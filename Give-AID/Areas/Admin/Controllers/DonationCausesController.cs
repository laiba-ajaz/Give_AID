using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class DonationCausesController : Controller
{
    private const string ImageFolder = "causes";

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public DonationCausesController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index()
    {
        var causes = _context.DonationCauses.ToList();
        return View(causes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCause(DonationCauses cause, IFormFile? ImageFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        cause.imageURL = null;
        if (ImageFile != null)
        {
            try { cause.imageURL = await _images.SaveAsync(ImageFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        _context.DonationCauses.Add(cause);
        _context.SaveChanges();
        TempData["Success"] = "Cause added successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCause(DonationCauses cause, IFormFile? ImageFile, bool RemoveImage = false)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        var existing = _context.DonationCauses.Find(cause.DonationCausesID);
        if (existing == null)
        {
            TempData["Error"] = "Cause not found.";
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
            _images.Delete(existing.imageURL);
            existing.imageURL = newPath;
        }
        else if (RemoveImage)
        {
            _images.Delete(existing.imageURL);
            existing.imageURL = null;
        }
        // warna purani image waisi hi rahegi

        existing.CauseName = cause.CauseName;
        existing.CauseDescription = cause.CauseDescription;
        existing.isActive = cause.isActive;

        _context.SaveChanges();
        TempData["Success"] = "Cause updated successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DltCause(int id)
    {
        var cause = _context.DonationCauses.Find(id);
        if (cause == null)
        {
            TempData["Error"] = "Cause not found.";
            return RedirectToAction("Index");
        }

        if (_context.Programmes.Any(p => p.DonationCausesID == id))
        {
            TempData["Error"] = "This cause has programmes. Delete them first or deactivate the cause.";
            return RedirectToAction("Index");
        }

        _context.DonationCauses.Remove(cause);
        _context.SaveChanges();
        _images.Delete(cause.imageURL);
        TempData["Success"] = "Cause deleted successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var cause = _context.DonationCauses.Find(id);
        if (cause == null)
        {
            TempData["Error"] = "Cause not found.";
            return RedirectToAction("Index");
        }

        cause.isActive = !cause.isActive;
        _context.SaveChanges();

        TempData["Success"] = cause.isActive ? "Cause activated." : "Cause deactivated.";
        return RedirectToAction("Index");
    }

}
