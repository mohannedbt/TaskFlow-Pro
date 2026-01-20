using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // -------------------------
        // REGISTER (GET)
        // /Account/Register?code=XXXX
        // -------------------------
        [HttpGet]
        public IActionResult Register(string? code = null)
        {
            var vm = new RegisterViewModel
            {
                InviteCode = code
            };
            return View(vm);
        }

        // -------------------------
        // REGISTER (POST)
        // -------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Owner without invite -> go workspace setup (intermediate step)
            if (vm.Role == SignupRole.Owner && string.IsNullOrWhiteSpace(vm.InviteCode))
            {
                TempData["pending_username"] = vm.Username;
                TempData["pending_email"] = vm.Email;
                TempData["pending_password"] = vm.Password;
                return RedirectToAction("Setup", "Workspace");
            }

            // Admin/Member MUST have invite
            if (vm.Role != SignupRole.Owner && string.IsNullOrWhiteSpace(vm.InviteCode))
            {
                ModelState.AddModelError("", "Admin/Member registration requires an invite link/code.");
                return View(vm);
            }

            // Invite verification path
            var invite = await _db.WorkspaceInvites
                .Include(i => i.Workspace)
                .FirstOrDefaultAsync(i => i.Code == vm.InviteCode);

            if (invite == null || invite.Used || invite.ExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired invite code.");
                return View(vm);
            }

            // Optional: invite email lock
            if (!string.IsNullOrWhiteSpace(invite.Email) &&
                !string.Equals(invite.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "This invite is not assigned to this email.");
                return View(vm);
            }

            // Verify email pattern
            if (!EmailMatchesPattern(vm.Email, invite.Workspace.EmailPattern))
            {
                ModelState.AddModelError("", "Email does not match this workspace policy.");
                return View(vm);
            }

            // Verify max members
            var membersCount = await _db.Users.CountAsync(u => u.WorkspaceId == invite.WorkspaceId);
            if (membersCount >= invite.Workspace.MaxMembers)
            {
                ModelState.AddModelError("", "This workspace has reached its maximum number of members.");
                return View(vm);
            }

            // SECURITY: role comes from invite (not from user dropdown)
            // Only "Admin" or "Member"
            var roleToGrant = NormalizeInviteRole(invite.RoleToGrant);

            // Create user
            var user = new ApplicationUser
            {
                UserName = vm.Username,
                Email = vm.Email,
                WorkspaceId = invite.WorkspaceId
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, roleToGrant);

            invite.Used = true;
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Mytasks", "Task");
        }

        // -------------------------
        // Helpers
        // -------------------------
        private static bool EmailMatchesPattern(string email, string pattern)
        {
            try
            {
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeInviteRole(string? role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)) return "Admin";
            return "Member";
        }
        // -------------------------
        // LOGIN (GET)
        // /Account/Login?returnUrl=...
        // -------------------------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // -------------------------
        // LOGIN (POST)
        // -------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Optional: find user first (gives nicer error handling)
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Sign in using Identity
            var result = await _signInManager.PasswordSignInAsync(
                userName: user.UserName!,
                password: model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // If returnUrl is local, go there; otherwise home.
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("MyTasks", "Task");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account is temporarily locked. Try again later.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Optional: logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
    
}
