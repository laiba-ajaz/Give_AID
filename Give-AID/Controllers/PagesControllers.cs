using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Data;
using Give_AID.Models;

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

