using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class PartnersController : Controller
{
    private const string ImageFolder = "partners";

    private readonly AppDBContext _context;
    private readonly ImageService _images;

    public PartnersController(AppDBContext context, ImageService images)
    {
        _context = context;
        _images = images;
    }

    public IActionResult Index()
    {
        var partners = _context.Partners.OrderBy(p => p.PartnerName).ToList();
        return View(partners);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPartner(Partner partner, IFormFile? LogoFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        partner.LogoPath = null;
        if (LogoFile != null)
        {
            try { partner.LogoPath = await _images.SaveAsync(LogoFile, ImageFolder); }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        _context.Partners.Add(partner);
        _context.SaveChanges();
        TempData["Success"] = "Partner added successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Partner partner, IFormFile? LogoFile, bool RemoveImage = false)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        var existing = _context.Partners.Find(partner.PartnerId);
        if (existing == null)
        {
            TempData["Error"] = "Partner not found.";
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

        existing.PartnerName = partner.PartnerName;
        existing.Description = partner.Description;
        existing.WebsiteUrl = partner.WebsiteUrl;
        existing.IsActive = partner.IsActive;

        _context.SaveChanges();
        TempData["Success"] = "Partner updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DltPartner(int id)
    {
        var partner = _context.Partners.Find(id);
        if (partner == null)
        {
            TempData["Error"] = "Partner not found.";
            return RedirectToAction("Index");
        }


        _context.Partners.Remove(partner);
        _context.SaveChanges();
        _images.Delete(partner.LogoPath);
        TempData["Success"] = "Partner deleted successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var partner = _context.Partners.Find(id);
        if (partner == null)
        {
            TempData["Error"] = "Partner not found.";
            return RedirectToAction("Index");
        }

        partner.IsActive = !partner.IsActive;
        _context.SaveChanges();

        TempData["Success"] = partner.IsActive ? "Partner activated." : "Partner deactivated.";
        return RedirectToAction("Index");
    }


}
