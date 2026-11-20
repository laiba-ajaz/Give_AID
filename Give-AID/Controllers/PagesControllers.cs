using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Controllers;

// UI and User Module controllers for Give-AID application screens.

// Handles donation page loading, card validation, payment recording, and donor history
public class DonateController : Controller
{
    // Database context for accessing DonationCauses, Donations, and Users
    private readonly AppDBContext _context;

    // Constructor to inject the database context
    public DonateController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Donate - Displays the donation form populated with active causes from the database
    [HttpGet]
    public async Task<IActionResult> Index(string? cause = null)
    {
        // 1. Fetch all active donation causes from the database
        var activeCauses = await _context.DonationCauses
            .Where(c => c.isActive)
            .ToListAsync();

        // 2. Prepare the view model with active causes and default amounts
        var model = new DonationViewModel
        {
            ActiveCauses = activeCauses,
            Amount = 50.00m,
            Frequency = "OneTime",
            CoverFees = true,
            TotalAmount = 51.50m,
            CardType = "Credit"
        };

        // 3. If a specific cause parameter was passed via URL (e.g., ?cause=education), select it
        if (!string.IsNullOrWhiteSpace(cause))
        {
            var matched = activeCauses.FirstOrDefault(c =>
                c.DonationCausesID.ToString() == cause ||
                c.CauseName.ToLower().Contains(cause.ToLower()));

            if (matched != null)
            {
                model.Cause = matched.DonationCausesID.ToString();
            }
        }
        else if (activeCauses.Any())
        {
            // Default to the first available cause
            model.Cause = activeCauses.First().DonationCausesID.ToString();
        }

        return View(model);
    }

    // POST: /Donate - Validates the donation form, verifies card details, and saves the donation record
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DonationViewModel model)
    {
        // 1. Fetch active causes again to populate the form if we need to return errors
        model.ActiveCauses = await _context.DonationCauses
            .Where(c => c.isActive)
            .ToListAsync();

        // 2. Validate that an active cause was selected
        DonationCauses? selectedCause = null;
        if (!string.IsNullOrWhiteSpace(model.Cause))
        {
            selectedCause = model.ActiveCauses.FirstOrDefault(c =>
                c.DonationCausesID.ToString() == model.Cause ||
                c.CauseName.Equals(model.Cause, StringComparison.OrdinalIgnoreCase) ||
                c.CauseName.ToLower().Contains(model.Cause.ToLower()));
        }

        if (selectedCause == null)
        {
            ModelState.AddModelError("Cause", "Please choose a valid active cause from the list.");
        }

        // 3. Validate donation amount
        if (model.Amount <= 0)
        {
            ModelState.AddModelError("Amount", "Donation amount must be greater than zero.");
        }

        // 4. Validate cardholder name
        if (string.IsNullOrWhiteSpace(model.CardHolderName))
        {
            ModelState.AddModelError("CardHolderName", "Cardholder name is required.");
        }

        // 5. Validate card number using the standard Luhn algorithm (checksum)
        string cleanCardDigits = Regex.Replace(model.CardNumber ?? string.Empty, @"\D", "");
        if (string.IsNullOrEmpty(cleanCardDigits) || !IsValidLuhn(cleanCardDigits))
        {
            ModelState.AddModelError("CardNumber", "Please enter a valid card number.");
        }

        // 6. Validate expiration date format (MM/YY) and check that it is not expired
        if (string.IsNullOrWhiteSpace(model.ExpiryDate) || !Regex.IsMatch(model.ExpiryDate.Trim(), @"^(0[1-9]|1[0-2])\/\d{2}$"))
        {
            ModelState.AddModelError("ExpiryDate", "Expiration date must be in MM/YY format.");
        }
        else
        {
            var parts = model.ExpiryDate.Trim().Split('/');
            int month = int.Parse(parts[0]);
            int year = 2000 + int.Parse(parts[1]);

            // Expiration occurs at the end of the specified month
            var expirationDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            if (expirationDate < DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "This payment card has expired.");
            }
        }

        // 7. Validate CVV security code (must be 3 or 4 numeric digits)
        string cleanCvv = Regex.Replace(model.Cvv ?? string.Empty, @"\D", "");
        if (string.IsNullOrEmpty(cleanCvv) || (cleanCvv.Length != 3 && cleanCvv.Length != 4))
        {
            ModelState.AddModelError("Cvv", "Security code (CVV) must be 3 or 4 digits.");
        }

        // 8. If validation failed, return the view with the error messages
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 9. Get the current logged-in user's ID from authentication claims
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Donate" });
        }

        // 10. Calculate final total amount (including optional 3% processing fee)
        decimal calculatedTotal = model.CoverFees ? Math.Round(model.Amount * 1.03m, 2) : model.Amount;

        // 11. Extract only the last 4 digits for PCI security compliance (never store full card numbers)
        string last4Digits = cleanCardDigits.Length >= 4
            ? cleanCardDigits.Substring(cleanCardDigits.Length - 4)
            : cleanCardDigits;

        // 12. Generate a unique transaction reference number
        string transactionId = $"TXN-GIVE-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";

        // 13. Create and save the Donation record in the database
        var donation = new Donation
        {
            UserId = userId,
            DonationCausesID = selectedCause!.DonationCausesID,
            Amount = model.Amount,
            CoverFees = model.CoverFees,
            TotalCharged = calculatedTotal,
            Frequency = model.Frequency == "Monthly" ? "Monthly" : "Once",
            PaymentMethod = model.CardType == "Debit" ? "Debit" : "Credit",
            CardLast4 = last4Digits,
            TransactionId = transactionId,
            Status = "Success",
            CreatedAt = DateTime.Now
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        // 14. Populate success properties to trigger the confirmation modal in the view
        model.IsSuccess = true;
        model.TransactionId = transactionId;
        model.ConfirmedCauseName = selectedCause.CauseName;
        model.DonorName = User.FindFirstValue(ClaimTypes.Name) ?? model.CardHolderName;
        model.ConfirmedTotalAmount = calculatedTotal;

        return View(model);
    }

    // GET: /Donate/History - Displays a list of all past donations made by the logged-in user
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History()
    {
        // 1. Get logged-in user's ID
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Donate/History" });
        }

        // 2. Fetch the user's donations ordered by most recent first
        var donations = await _context.Donations
            .Include(d => d.DonationCause)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonationHistoryItem
            {
                DonationId = d.DonationId,
                Date = d.CreatedAt,
                CauseName = d.DonationCause != null ? d.DonationCause.CauseName : "Community Welfare",
                Amount = d.Amount,
                TotalCharged = d.TotalCharged,
                Frequency = d.Frequency,
                PaymentMethod = d.PaymentMethod,
                CardLast4 = d.CardLast4,
                TransactionId = d.TransactionId,
                Status = d.Status
            })
            .ToListAsync();

        return View(donations);
    }

    // Helper method: Implements Luhn algorithm to check if card number digits are mathematically valid
    private static bool IsValidLuhn(string digits)
    {
        if (digits.Length < 13 || digits.Length > 19) return false;

        int sum = 0;
        bool alternate = false;

        // Traverse digits from right to left
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int digit = digits[i] - '0';
            if (digit < 0 || digit > 9) return false;

            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}

// GET & POST: /Programmes — upcoming programmes & events + user participation interest form
public class ProgrammesController : Controller
{
    // Database context for querying Programmes and saving ProgrammeInterest
    private readonly AppDBContext _context;

    // Constructor to inject the database context
    public ProgrammesController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Programmes - Loads active programmes from the database and renders the programmes directory
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 1. Fetch active programmes including related NGO and Cause data
        var programmes = await _context.Programmes
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .Where(p => p.isActive)
            .OrderBy(p => p.StartDate)
            .ToListAsync();

        // 2. Pre-fill name and email if the current user is logged in
        string defaultName = string.Empty;
        string defaultEmail = string.Empty;
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            defaultName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            defaultEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        // 3. Build the view model with active programmes and default selection
        var model = new ProgrammeIndexViewModel
        {
            ActiveProgrammes = programmes,
            ProgrammeId = programmes.FirstOrDefault()?.ProgrammeId ?? 0,
            FullName = defaultName,
            Email = defaultEmail,
            ParticipationRole = "Volunteer"
        };

        return View(model);
    }

    // POST: /Programmes/Interest - Handles registration of interest for a programme by an authenticated user
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Interest(ProgrammeIndexViewModel model)
    {
        // 1. Reload active programmes for view re-rendering in case of validation errors
        model.ActiveProgrammes = await _context.Programmes
            .Include(p => p.NGO)
            .Include(p => p.Cause)
            .Where(p => p.isActive)
            .OrderBy(p => p.StartDate)
            .ToListAsync();

        // 2. Validate participant name
        if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Trim().Length < 2)
        {
            ModelState.AddModelError("FullName", "Please enter your full name (at least 2 characters).");
        }

        // 3. Validate participant email address
        if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
        {
            ModelState.AddModelError("Email", "Please enter a valid email address.");
        }

        // 4. Validate targeted programme exists and is active
        var programme = model.ActiveProgrammes.FirstOrDefault(p => p.ProgrammeId == model.ProgrammeId);
        if (programme == null)
        {
            ModelState.AddModelError("ProgrammeId", "The selected programme does not exist or is inactive.");
        }

        // 5. Get authenticated user's ID
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Programmes" });
        }

        // 6. Prevent the same user from registering interest twice for the same programme
        bool alreadyRegistered = await _context.ProgrammeInterests
            .AnyAsync(pi => pi.ProgrammeId == model.ProgrammeId && pi.UserId == userId);

        if (alreadyRegistered)
        {
            model.ErrorMessage = "You have already registered your interest for this programme.";
            ModelState.AddModelError("", model.ErrorMessage);
            return View("Index", model);
        }

        // 7. If model validation fails, re-render the index view with errors
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // 8. Save the user's programme interest record to the database
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

        // 9. Mark view as successful and provide feedback
        model.IsSuccess = true;
        model.SuccessMessage = $"Your interest for \"{programme!.Title}\" has been registered successfully! The coordinator will be in touch.";

        return View("Index", model);
    }
}

// GET & POST: /HelpCentre — FAQs, contact directory, and raise a query support desk
public class HelpCentreController : Controller
{
    // Database context for storing queries
    private readonly AppDBContext _context;

    // Constructor to inject the database context
    public HelpCentreController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /HelpCentre - Displays the help desk, FAQs, and query submission form
    [HttpGet]
    public IActionResult Index()
    {
        // 1. Initialize view model
        var model = new HelpCentreViewModel();

        // 2. Pre-fill name and email if the current user is logged in
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            model.FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            model.Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        return View(model);
    }

    // POST: /HelpCentre/Query - Receives, validates, and records public or donor support queries
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Query(HelpCentreViewModel model)
    {
        // 1. Validate participant name
        if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Trim().Length < 2)
        {
            ModelState.AddModelError("FullName", "Please enter your full name (at least 2 characters).");
        }

        // 2. Validate participant email
        if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
        {
            ModelState.AddModelError("Email", "Please enter a valid email address.");
        }

        // 3. Validate inquiry message length (minimum 10 characters required)
        if (string.IsNullOrWhiteSpace(model.Message) || model.Message.Trim().Length < 10)
        {
            ModelState.AddModelError("Message", "Your inquiry message must be at least 10 characters.");
        }

        // 4. Return view with validation errors if any check fails
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // 5. Retrieve logged-in user ID if authenticated (nullable for guest inquiries)
        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
        {
            userId = parsedId;
        }

        // 6. Map and create the Query entity with default Status "New"
        var query = new Query
        {
            UserId = userId,
            Name = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Topic = string.IsNullOrWhiteSpace(model.Topic) ? "donation" : model.Topic.Trim(),
            Message = model.Message.Trim(),
            Status = "New",
            CreatedAt = DateTime.Now
        };

        // 7. Save query record to the database
        _context.Queries.Add(query);
        await _context.SaveChangesAsync();

        // 8. Generate inquiry reference number and provide user feedback
        string refNumber = $"#GA-{query.QueryId:D4}";
        model.IsSuccess = true;
        model.ReferenceNumber = refNumber;
        model.SuccessMessage = $"Your query has been dispatched under reference {refNumber}. A welfare coordinator will review your request.";

        return View("Index", model);
    }
}

// GET & POST: /Invite — invite friends via email and share referral link
public class InviteController : Controller
{
    // Database context for recording invitations
    private readonly AppDBContext _context;

    // Email service interface for dispatching notifications
    private readonly IEmailSender _emailSender;

    // Constructor injecting DbContext and EmailSender service
    public InviteController(AppDBContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    // GET: /Invite - Displays referral links and recipient invitation staging form
    [HttpGet]
    public IActionResult Index()
    {
        return View(new InviteViewModel());
    }

    // POST: /Invite/Index - Receives staged recipient emails, validates, removes duplicates, and saves invitations
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InviteViewModel model)
    {
        // 1. Check if any emails were provided
        if (model.Emails == null || !model.Emails.Any())
        {
            model.ErrorMessage = "Please add at least one recipient email address.";
            ModelState.AddModelError("Emails", model.ErrorMessage);
            return View(model);
        }

        // 2. Normalize and remove duplicate email entries
        var emailValidator = new EmailAddressAttribute();
        var uniqueCleanEmails = model.Emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLower())
            .Distinct()
            .Where(e => emailValidator.IsValid(e))
            .ToList();

        // 3. Ensure at least one valid recipient remains
        if (!uniqueCleanEmails.Any())
        {
            model.ErrorMessage = "Please provide valid email address formats.";
            ModelState.AddModelError("Emails", model.ErrorMessage);
            return View(model);
        }

        // 4. Retrieve logged-in user ID from claims
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Invite" });
        }

        string defaultMessage = !string.IsNullOrWhiteSpace(model.Message)
            ? model.Message.Trim()
            : "Hey! Join me on Give-AID to support verified grassroots welfare programs.";

        // 5. Save one invitation record per unique recipient
        foreach (var email in uniqueCleanEmails)
        {
            var invitation = new Invitation
            {
                UserId = userId,
                FriendEmail = email,
                Message = defaultMessage,
                SentAt = DateTime.Now
            };

            _context.Invitations.Add(invitation);

            // 6. Simulate email dispatch via the configured IEmailSender service
            await _emailSender.SendEmailAsync(
                email,
                "You have been invited to join Give-AID!",
                defaultMessage
            );
        }

        // 7. Commit changes to the database
        await _context.SaveChangesAsync();

        // 8. Provide confirmation feedback to the view
        model.IsSuccess = true;
        model.SentCount = uniqueCleanEmails.Count;
        model.SuccessMessage = $"{uniqueCleanEmails.Count} Invitation{(uniqueCleanEmails.Count == 1 ? "" : "s")} Dispatched!";
        model.Emails.Clear(); // Clear staged recipients upon successful send

        return View(model);
    }
}

