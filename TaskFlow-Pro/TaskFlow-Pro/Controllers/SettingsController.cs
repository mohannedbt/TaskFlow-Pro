using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public SettingsController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // =========================
    // VIEW SETTINGS
    // =========================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var team = user.TeamId != null
            ? await _context.Teams.FindAsync(user.TeamId)
            : null;

        var model = new SettingsViewModel
        {
            Username = user.UserName!,
            Email = user.Email!,
            TeamName = team?.Name,

            // Defaults for now (can be stored later)
            Theme = "Light",
            CalendarView = "Month",
            CompactCalendar = false,
            EmailNotifications = true,
            TaskAssignedNotification = true,
            DueDateReminder = true
        };

        return View(model);
    }

    // =========================
    // SAVE SETTINGS
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.UserName = model.Username;
        user.Email = model.Email;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        ViewBag.Success = "Settings saved successfully.";
        return View(model);
    }
}
