using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Give_AID.Models;
using Give_AID.Data;

namespace Give_AID.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public AccountController(AppDBContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        public IActionResult Login() => View("Register");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", user);
            }

            user.Email = user.Email.Trim();

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View("Register", user);
            }

            user.Role = "Donor";
            user.Password = _hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.LoginError = "Please enter your email and password.";
                return View("Register");
            }

            // 1. Email se user dhoondo
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            // 2. Password check karo (hash se compare)
            if (user == null || !CheckPassword(user, password))
            {
                ViewBag.LoginError = "Invalid email or password.";
                return View("Register");
            }

            // 3. User ki info (claims) cookie mein store karo
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Donor")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = rememberMe });

            // 4. Redirect: pehle wale page par, ya role ke hisaab se
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }

        // Password verify karta hai. Purane users (jin ka password plain text mein save hai)
        // ke liye plain compare karta hai aur sahi hone par password ko hash karke update kar deta hai.
        private bool CheckPassword(User user, string password)
        {
            if (IsHashed(user.Password))
            {
                var result = _hasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = _hasher.HashPassword(user, password);
                    _context.SaveChanges();
                }
                return result != PasswordVerificationResult.Failed;
            }

            // Stored password hash nahi hai (purana plain-text record)
            if (user.Password != password) return false;

            user.Password = _hasher.HashPassword(user, password);
            _context.SaveChanges();
            return true;
        }

        // PasswordHasher ka hash Base64 hota hai, decode hone par pehla byte 0x00 (V2) ya 0x01 (V3) hota hai
        private static bool IsHashed(string stored)
        {
            var buffer = new byte[stored.Length];
            if (!Convert.TryFromBase64String(stored, buffer, out int len) || len == 0) return false;
            return (buffer[0] == 0x01 && len >= 61) || (buffer[0] == 0x00 && len == 49);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
