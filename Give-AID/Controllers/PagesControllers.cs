using Microsoft.AspNetCore.Mvc;

namespace Give_AID.Controllers;

// UI-only controllers for the Stitch screens. Add POST actions / models here when wiring the backend.

// GET: /Donate  — choose cause, amount, card payment
public class DonateController : Controller
{
    public IActionResult Index() => View();
}

// GET: /Programmes  — upcoming programmes & events + participation form
public class ProgrammesController : Controller
{
    public IActionResult Index() => View();
}

// GET: /HelpCentre  — FAQs, raise a query, contact us
public class HelpCentreController : Controller
{
    public IActionResult Index() => View();
}

// GET: /Invite  — invite friends by email
public class InviteController : Controller
{
    public IActionResult Index() => View();
}

