using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =========================
        // REGISTER (GET)
        // =========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =========================
        // REGISTER (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ✅ YOU MUST DO THIS:
            // Always validate the incoming model
            if (!ModelState.IsValid)
                return View(model);

            // ✅ YOU MUST DO THIS:
            // Create the Identity user manually
            var user = new ApplicationUser
            {
                UserName = model.Username,   // Identity requires UserName
                Email = model.Email
            };

            // 🔥 Identity validation happens HERE (password rules, duplicates, etc.)
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // ✅ OPTIONAL BUT RECOMMENDED:
                // Auto-login after successful registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("MyTasks", "Task");
            }

            // 🔴 YOU MUST DO THIS:
            // Identity errors are NOT tied to a field → must be shown via ValidationSummary
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Return view WITH errors
            return View(model);
        }

        // =========================
        // LOGIN (GET)
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔎 Find user by email FIRST
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // ✅ Use UserName internally
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
                return RedirectToAction("MyTasks", "Task");

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }


        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
