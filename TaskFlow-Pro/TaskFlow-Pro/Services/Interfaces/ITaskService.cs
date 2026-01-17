using TaskFlow_Pro.Models;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface ITaskService
    {
        // ========================
        // Creation
        // ========================

        Task<TaskItem> CreateTaskAsync(string title, string description, string creatorUserId, DateTime startDate,
            DateTime endDate);
        // ========================
        // Assignment
        // ========================

        Task AssignTaskAsync(
            int taskId,
            ApplicationUser assignedUser
        );

        // ========================
        // State management
        // ========================

        Task ChangeTaskStateAsync(
            int taskId,
            State newState,
            string actingUserId
        );
        Task<List<TaskItem>> GetAllTasksAsync();
        Task<List<TaskItem>> SearchTasksAsync(string q);

    }
}