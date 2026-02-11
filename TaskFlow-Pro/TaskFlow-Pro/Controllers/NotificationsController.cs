using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notif;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(INotificationService notif, UserManager<ApplicationUser> userManager)
        {
            _notif = notif;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var list = await _notif.GetAllAsync(userId, take: 80);
            ViewBag.UnreadCount = await _notif.GetUnreadCountAsync(userId);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _notif.MarkAsReadAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User)!;
            await _notif.MarkAllAsReadAsync(userId);
            return RedirectToAction(nameof(Index));
        }
    }
}
