using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.ViewComponents
{
    public class NotificationBadgeViewComponent : ViewComponent
    {
        private readonly INotificationService _notif;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationBadgeViewComponent(
            INotificationService notif,
            UserManager<ApplicationUser> userManager)
        {
            _notif = notif;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View(0);

            var userId = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User)!;
            var count = await _notif.GetUnreadCountAsync(userId);
            return View(count);
        }
    }
}
