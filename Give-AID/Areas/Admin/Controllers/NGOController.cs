using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class NGOController : Controller
{
   
    private readonly AppDBContext _context;

    public NGOController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var ngosName = _context.NGOs.ToList();
        return View(ngosName);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddNgo(NGO nGO)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Index");
        }

        _context.NGOs.Add(nGO);
        _context.SaveChanges();
        TempData["Success"] = "NGO added successfully.";
        return RedirectToAction("Index");
    }

   
     [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(NGO nGO)
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

        existing.NGOName = nGO.NGOName;
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
