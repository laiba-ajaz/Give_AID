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

// =========================================================================
// DONATE CONTROLLER (User Module - Feature 1)
// Donors yahan se apni pasandeeda cause chunte hain aur card se donate karte hain.
// Payment validation, fee calculation, aur transaction history yahan handle hoti hai.
// =========================================================================
public class DonateController : Controller
{
    private readonly AppDBContext _context;

    public DonateController(AppDBContext context)
    {
        _context = context;
    }

    // GET: /Donate - Donation form load karta hai active causes ke sath
    [HttpGet]
    public async Task<IActionResult> Index(string? cause = null)
    {
        // Active causes database se fetch karein
        var activeCauses = await _context.DonationCauses
            .Where(c => c.isActive)
            .OrderBy(c => c.CauseName)
            .ToListAsync();

        var model = new DonationViewModel
        {
            ActiveCauses = activeCauses,
            Amount = 50.00m,
            Frequency = "OneTime",
            CoverFees = true,
            TotalAmount = 51.50m,
            CardType = "Credit"
        };

        // Agar URL mein cause di ho (e.g. ?cause=3) to wahi pehle se select karein
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
            model.Cause = activeCauses.First().DonationCausesID.ToString();
        }

        return View(model);
    }

    // POST: /Donate - Form submit par card details validate karta hai aur DB mein donation save karta hai
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DonationViewModel model)
    {
        // Validation fail hone par dropdown ke liye dobara causes load karein
        model.ActiveCauses = await _context.DonationCauses
            .Where(c => c.isActive)
            .OrderBy(c => c.CauseName)
            .ToListAsync();

        // 1. Check karein valid active cause select hui hai
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

        // 2. Donation amount check (positive honi chahiye)
        if (model.Amount <= 0)
        {
            ModelState.AddModelError("Amount", "Donation amount must be greater than zero.");
        }

        // 3. Cardholder name check
        if (string.IsNullOrWhiteSpace(model.CardHolderName))
        {
            ModelState.AddModelError("CardHolderName", "Cardholder name is required.");
        }

        // 4. Card number ko standard Luhn algorithm se verify karein
        string cleanCardDigits = Regex.Replace(model.CardNumber ?? string.Empty, @"\D", "");
        if (string.IsNullOrEmpty(cleanCardDigits) || !IsValidLuhn(cleanCardDigits))
        {
            ModelState.AddModelError("CardNumber", "Please enter a valid card number.");
        }

        // 5. Card expiry date (MM/YY format aur future date honi chahiye)
        if (string.IsNullOrWhiteSpace(model.ExpiryDate) || !Regex.IsMatch(model.ExpiryDate.Trim(), @"^(0[1-9]|1[0-2])\/?([0-9]{2})$"))
        {
            ModelState.AddModelError("ExpiryDate", "Enter a valid expiration date (MM/YY).");
        }
        else
        {
            var match = Regex.Match(model.ExpiryDate.Trim(), @"^(0[1-9]|1[0-2])\/?([0-9]{2})$");
            int expMonth = int.Parse(match.Groups[1].Value);
            int expYear = 2000 + int.Parse(match.Groups[2].Value);
            var now = DateTime.UtcNow;

            if (expYear < now.Year || (expYear == now.Year && expMonth < now.Month))
            {
                ModelState.AddModelError("ExpiryDate", "The payment card has expired.");
            }
        }

        // 6. CVV (3 ya 4 digits)
        string cleanCvv = (model.Cvv ?? string.Empty).Trim();
        if (cleanCvv.Length < 3 || cleanCvv.Length > 4 || !cleanCvv.All(char.IsDigit))
        {
            ModelState.AddModelError("Cvv", "CVV must be 3 or 4 numeric digits.");
        }

        // 7. Agar koi validation fail hui ho to wapis form dikhayein
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 8. Logged-in user ka ID claims se lein
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Donate" });
        }

        // 9. Total amount calculate karein (optional 3% transaction fee ke sath)
        decimal calculatedTotal = model.CoverFees ? Math.Round(model.Amount * 1.03m, 2) : model.Amount;

        // Security: Card ke sirf aakhri 4 digits store karein (PCI compliance)
        string last4Digits = cleanCardDigits.Length >= 4
            ? cleanCardDigits.Substring(cleanCardDigits.Length - 4)
            : cleanCardDigits;

        // Unique transaction reference number
        string transactionId = $"TXN-GIVE-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";

        // 10. Donation database mein save karein
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

        // 11. Modal popup ke liye success data set karein
        model.IsSuccess = true;
        model.TransactionId = transactionId;
        model.ConfirmedCauseName = selectedCause.CauseName;
        model.DonorName = User.FindFirstValue(ClaimTypes.Name) ?? model.CardHolderName;
        model.ConfirmedTotalAmount = calculatedTotal;

        return View(model);
    }

    // GET: /Donate/History - User ki tamam purani donations ki list dikhata hai
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Donate/History" });
        }

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

    // Helper: Luhn algorithm check karta hai ke credit/debit card number mathematically theek hai ya nahi
    private static bool IsValidLuhn(string digits)
    {
        if (digits.Length < 13 || digits.Length > 19) return false;

        int sum = 0;
        bool alternate = false;

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

// =========================================================================
// INVITE CONTROLLER (User Module - Feature 4)
// Users yahan se doston ko email invite bhejte hain (Brevo SMTP ke zariye).
// Invitations DB mein save hoti hain aur real HTML email dispatch hota hai.
// =========================================================================
public class InviteController : Controller
{
    private readonly AppDBContext _context;
    private readonly IEmailSender _emailSender;

    public InviteController(AppDBContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    // GET: /Invite - Referral link aur email staging form load karta hai
    [HttpGet]
    public IActionResult Index()
    {
        return View(new InviteViewModel());
    }

    // POST: /Invite/Index - Emails par invitation dispatch karta hai aur DB mein record karta hai
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InviteViewModel model)
    {
        // 1. Check karein kam az kam ek email address diya gaya ho
        if (model.Emails == null || !model.Emails.Any())
        {
            model.ErrorMessage = "Please add at least one recipient email address.";
            ModelState.AddModelError("Emails", model.ErrorMessage);
            return View(model);
        }

        // 2. Duplicate emails hatayein aur format validate karein
        var emailValidator = new EmailAddressAttribute();
        var uniqueCleanEmails = model.Emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLower())
            .Distinct()
            .Where(e => emailValidator.IsValid(e))
            .ToList();

        if (!uniqueCleanEmails.Any())
        {
            model.ErrorMessage = "Please provide valid email address formats.";
            ModelState.AddModelError("Emails", model.ErrorMessage);
            return View(model);
        }

        // 3. Current user ka ID lein
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Invite" });
        }

        string defaultMessage = !string.IsNullOrWhiteSpace(model.Message)
            ? model.Message.Trim()
            : "Hey! Join me on Give-AID to support verified grassroots welfare programs.";

        // 4. Har recipient ke liye invitation DB mein save karein aur Brevo SMTP se email bhejein
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

            // Brevo SMTP relay ke zariye real HTML email dispatch karein
            await _emailSender.SendEmailAsync(
                email,
                "You have been invited to join Give-AID!",
                defaultMessage
            );
        }

        await _context.SaveChangesAsync();

        // 5. Success confirmation
        model.IsSuccess = true;
        model.SentCount = uniqueCleanEmails.Count;
        model.SuccessMessage = $"{uniqueCleanEmails.Count} Invitation{(uniqueCleanEmails.Count == 1 ? "" : "s")} Dispatched!";
        model.Emails.Clear();

        return View(model);
    }
}
