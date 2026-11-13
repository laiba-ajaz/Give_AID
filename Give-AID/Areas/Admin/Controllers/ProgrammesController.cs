using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class ProgrammesController : Controller
{
    private readonly AppDBContext _context;

    public ProgrammesController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var programmes = _context.Programmes
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .OrderByDescending(p => p.StartDate)
            .ToList();

        // Add/Edit form ke dropdowns ke liye
        ViewBag.NGOs = _context.NGOs.Where(n => n.isActive).OrderBy(n => n.NGOName).ToList();
        ViewBag.Causes = _context.DonationCauses.Where(c => c.isActive).OrderBy(c => c.CauseName).ToList();

        return View(programmes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProgramme(Programme programme)
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

        _context.Programmes.Add(programme);
        _context.SaveChanges();
        TempData["Success"] = "Programme added successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditProgramme(Programme programme)
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

        existing.Title = programme.Title;
        existing.Description = programme.Description;
        existing.ImagePath = programme.ImagePath;
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
