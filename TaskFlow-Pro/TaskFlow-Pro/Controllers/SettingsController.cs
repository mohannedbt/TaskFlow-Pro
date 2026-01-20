using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public SettingsController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = await _db.Users
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (me == null) return Unauthorized();

            var vm = new SettingsViewModel
            {
                Username = me.UserName ?? "",
                Email = me.Email ?? "",

                Theme = me.Theme,
                DefaultCalendarView = me.DefaultCalendarView,
                CompactCalendarMode = me.CompactCalendarMode,

                EmailNotifications = me.EmailNotifications,
                TaskAssignmentAlerts = me.TaskAssignmentAlerts,
                DueDateReminders = me.DueDateReminders,

                TeamName = me.Team?.Name
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SettingsViewModel vm)
        {
            var me = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (me == null) return Unauthorized();

            // Save in DB (good to keep)
            me.UserName = vm.Username?.Trim();

            me.Theme = vm.Theme;
            me.DefaultCalendarView = vm.DefaultCalendarView;
            me.CompactCalendarMode = vm.CompactCalendarMode;

            me.EmailNotifications = vm.EmailNotifications;
            me.TaskAssignmentAlerts = vm.TaskAssignmentAlerts;
            me.DueDateReminders = vm.DueDateReminders;

            await _db.SaveChangesAsync();

            // ✅ Save to cookies so UI can use them without DB queries
            var opt = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false, // must be readable by JS for calendar view
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = true // set false only if you're not using https locally
            };

            Response.Cookies.Append("tf_theme", me.Theme ?? "Light", opt);
            Response.Cookies.Append("tf_calview", me.DefaultCalendarView ?? "Month", opt);
            Response.Cookies.Append("tf_compact", me.CompactCalendarMode ? "1" : "0", opt);

            TempData["Success"] = "Settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
