using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Give_AID.Data;

namespace Give_AID.Controllers;

// GET:  /contact-us                 — contact details + message form
// GET:  /contact-us?subject=...     — form ka subject pehle se bhara hua (e.g. job apply)
// POST: /contact-us                 — message save
[Route("contact-us")]
public class ContactUsController : Controller
{
    private readonly AppDBContext _context;

    public ContactUsController(AppDBContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(string? subject)
    {
        var message = new ContactMessage { Subject = subject ?? "" };

        // Login user ka naam aur email khud bhar do
        if (User.Identity?.IsAuthenticated == true)
        {
            message.FullName = User.Identity.Name ?? "";
            message.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        }

        ViewBag.Info = LoadInfo();
        return View(message);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    // Parameter ka naam "message" nahi rakha kyunke form mein "Message" field bhi hai — dono mil kar binding bigaad dete hain
    public IActionResult Index([Bind("FullName,Email,Phone,Subject,Message")] ContactMessage contact, string? website)
    {
        // "website" field form mein chupa hua hai; insaan isay nahi bharta, spam bots bhar dete hain
        if (!string.IsNullOrEmpty(website))
        {
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Info = LoadInfo();
            return View(contact);
        }

        contact.FullName = contact.FullName.Trim();
        contact.Email = contact.Email.Trim();
        contact.Subject = contact.Subject.Trim();
        contact.CreatedAt = DateTime.Now;

        _context.ContactMessages.Add(contact);
        _context.SaveChanges();

        TempData["ContactSent"] = contact.FullName;
        return RedirectToAction("Index");
    }

    private ContactInfo LoadInfo() => _context.ContactInfos.FirstOrDefault() ?? new ContactInfo();
}
