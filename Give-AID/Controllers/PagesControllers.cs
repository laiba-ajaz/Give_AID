using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;

namespace Give_AID.Controllers;

// UI-only controllers for the Stitch screens. Add POST actions / models here when wiring the backend.

// GET: /Donate  — choose cause, amount, card payment
// Causes admin panel (Donation Causes) se aate hain; payment abhi save nahi hoti.
public class DonateController : Controller
{
    private readonly AppDBContext _context;

    public DonateController(AppDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var causes = _context.DonationCauses
            .Where(c => c.isActive)
            .OrderBy(c => c.CauseName)
            .ToList();
        return View(causes);
    }
}

// GET: /Invite  — invite friends by email
public class InviteController : Controller
{
    public IActionResult Index() => View();
}

