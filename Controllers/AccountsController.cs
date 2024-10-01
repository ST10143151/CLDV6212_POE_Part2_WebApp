using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ABC_Retailers.Models;
using ABC_Retailers.Services;

namespace ABC_Retailers.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // Registration actions
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.UserExistsAsync(model.RowKey))
                {
                    ModelState.AddModelError("", "User already exists.");
                    return View(model);
                }

                model.PasswordHash = HashPassword(model.PasswordHash);
                await _userService.AddUserAsync(model);

                // Store a welcome message in TempData and redirect to login page
                TempData["RegistrationSuccess"] = $"Welcome to ABC Retailers, {model.FullName}!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // Login actions
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
public async Task<IActionResult> Login(string username, string password)
{
    var user = await _userService.GetUserAsync(username);

    if (user == null || !VerifyPassword(password, user.PasswordHash))
    {
        ModelState.AddModelError("", "Invalid username or password.");
        return View();
    }

    // Create claims
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.NameIdentifier, user.RowKey) // Assuming RowKey is the unique identifier
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true, // Persistent cookie across sessions
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
    };

    // Sign in the user
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

    return RedirectToAction("Index", "Home");
}


        // Logout action
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return storedHash == enteredHash;
        }
    }
}
