using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class DonationCausesController : Controller
{
    private readonly AppDBContext _context;

    public DonationCausesController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var causes = _context.DonationCauses.ToList();
        return View(causes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddCause(DonationCauses cause)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        _context.DonationCauses.Add(cause);
        _context.SaveChanges();
        TempData["Success"] = "Cause added successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCause(DonationCauses cause)
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

        existing.CauseName = cause.CauseName;
        existing.CauseDescription = cause.CauseDescription;
        existing.imageURL = cause.imageURL;
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

        _context.DonationCauses.Remove(cause);
        _context.SaveChanges();
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
