using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

namespace Give_AID.Controllers;

// =========================================================================
// PROGRAMMES CONTROLLER (User Module - Feature 2)
// Welfare programmes ki directory dikhata hai aur users ko "Show Interest"
// ke zariye volunteer ya participant ke tor par register hone ki ijazat deta hai.
// =========================================================================
public class ProgrammesController : Controller
{
    private readonly AppDBContext _context;

    public ProgrammesController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Programmes - Tamam active programmes aur interest form render karta hai
    // GET: /Programmes?select=5 - Home page se kisi specific programme par click karein to wo pre-select hota hai
    public async Task<IActionResult> Index(int? select)
    {
        var today = DateTime.Today;

        // Tamam active programmes related NGO aur Cause ke sath load karein
        var all = await _context.Programmes
            .AsNoTracking()
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .Where(p => p.isActive)
            .ToListAsync();

        // Aane wale (aur running) programmes pehle, expire ho chuke baad mein
        var upcoming = all.Where(p => !IsFinished(p, today)).OrderBy(p => p.StartDate);
        var finished = all.Where(p => IsFinished(p, today)).OrderByDescending(p => p.StartDate);
        var sortedProgrammes = upcoming.Concat(finished).ToList();

        // Agar user login ho to uska naam aur email form mein pehle se bhar dein
        string defaultName = string.Empty;
        string defaultEmail = string.Empty;
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            defaultName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            defaultEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        int selectedId = select ?? sortedProgrammes.FirstOrDefault()?.ProgrammeId ?? 0;

        var model = new ProgrammeIndexViewModel
        {
            ActiveProgrammes = sortedProgrammes,
            ProgrammeId = selectedId,
            FullName = defaultName,
            Email = defaultEmail,
            ParticipationRole = "Volunteer"
        };

        ViewBag.NgoCount = await _context.NGOs.CountAsync(n => n.isActive);

        return View(model);
    }

    // POST: /Programmes/Interest - Volunteer participation interest ko database mein save karta hai
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Interest(ProgrammeIndexViewModel model)
    {
        var today = DateTime.Today;

        // Validation fail hone par view dobara render karne ke liye programmes reload karein
        var all = await _context.Programmes
            .AsNoTracking()
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .Where(p => p.isActive)
            .ToListAsync();

        model.ActiveProgrammes = all.Where(p => !IsFinished(p, today)).OrderBy(p => p.StartDate)
            .Concat(all.Where(p => IsFinished(p, today)).OrderByDescending(p => p.StartDate))
            .ToList();

        // 1. Participant name check (kam az kam 2 characters)
        if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Trim().Length < 2)
        {
            ModelState.AddModelError("FullName", "Please enter your full name (at least 2 characters).");
        }

        // 2. Email format check
        if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
        {
            ModelState.AddModelError("Email", "Please enter a valid email address.");
        }

        // 3. Programme exist karta hai ya nahi
        var programme = model.ActiveProgrammes.FirstOrDefault(p => p.ProgrammeId == model.ProgrammeId);
        if (programme == null)
        {
            ModelState.AddModelError("ProgrammeId", "The selected programme does not exist or is inactive.");
        }

        // 4. Current user ID
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Programmes" });
        }

        // 5. Ek user ek hi programme par do baar interest register na kar sake
        bool alreadyRegistered = await _context.ProgrammeInterests
            .AnyAsync(pi => pi.ProgrammeId == model.ProgrammeId && pi.UserId == userId);

        if (alreadyRegistered)
        {
            model.ErrorMessage = "You have already registered your interest for this programme.";
            ModelState.AddModelError("", model.ErrorMessage);
            return View("Index", model);
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // 6. Database table ProgrammeInterests mein record save karein
        var interest = new ProgrammeInterest
        {
            ProgrammeId = model.ProgrammeId,
            UserId = userId,
            Name = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone?.Trim(),
            ParticipationRole = model.ParticipationRole ?? "Volunteer",
            Message = model.Message?.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.ProgrammeInterests.Add(interest);
        await _context.SaveChangesAsync();

        // 7. View par success confirmation message dikhayein
        model.IsSuccess = true;
        model.SuccessMessage = $"Your interest for \"{programme!.Title}\" has been registered successfully! The coordinator will be in touch.";

        return View("Index", model);
    }

    public static bool IsFinished(Give_AID.Models.Programme p, DateTime today) => (p.EndDate ?? p.StartDate) < today;
}
