using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // =========================
        // VIEW PROFILE
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var team = user.TeamId.HasValue
                ? await _context.Teams.FindAsync(user.TeamId.Value)
                : null;

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                TeamId = user.TeamId,
                TeamName = team?.Name,
                AvailableTeams = await _context.Teams.ToListAsync()
            };

            return View(model);
        }

        // =========================
        // UPDATE PROFILE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTeams = await _context.Teams.ToListAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Update identity fields
            user.UserName = model.Username;
            user.Email = model.Email;
            user.TeamId = model.TeamId;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.AvailableTeams = await _context.Teams.ToListAsync();
                return View(model);
            }

            // Reload team name after save
            var team = model.TeamId.HasValue
                ? await _context.Teams.FindAsync(model.TeamId.Value)
                : null;

            model.TeamName = team?.Name;
            model.AvailableTeams = await _context.Teams.ToListAsync();

            ViewBag.Success = "Profile updated successfully.";

            return View(model);
        }
    }
}
