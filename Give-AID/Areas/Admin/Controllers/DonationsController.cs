using Microsoft.AspNetCore.Mvc;

namespace Give_AID.Areas.Admin.Controllers;

[Area("Admin")]
public class DonationsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
