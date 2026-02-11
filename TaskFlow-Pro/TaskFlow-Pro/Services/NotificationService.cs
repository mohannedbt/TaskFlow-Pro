using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;

using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db) => _db = db;

        public async Task CreateAsync(Notification n)
        {
            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
        }

        public Task<int> GetUnreadCountAsync(string userId)
        {
            return _db.Notifications.AsNoTracking()
                .CountAsync(x => x.UserId == userId && !x.IsRead);
        }

        public Task<List<Notification>> GetLatestAsync(string userId, int take = 10)
        {
            return _db.Notifications.AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public Task<List<Notification>> GetAllAsync(string userId, int take = 50)
        {
            return _db.Notifications.AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n == null) return;

            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _db.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }
}
