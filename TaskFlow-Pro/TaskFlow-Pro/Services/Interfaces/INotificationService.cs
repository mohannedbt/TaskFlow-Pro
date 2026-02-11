using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(Notification n);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetLatestAsync(string userId, int take = 10);
        Task<List<Notification>> GetAllAsync(string userId, int take = 50);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
