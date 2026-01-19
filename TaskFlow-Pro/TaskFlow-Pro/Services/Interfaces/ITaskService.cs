using TaskFlow_Pro.Models;
using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface ITaskService
    {
        // ========================
        // Creation
        // ========================

        Task<TaskItem> CreateTaskAsync(string title, string description, string? creatorUserId, DateTime startDate,
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

        Task<List<TaskItem>> SearchTasksAsync(string keyword);

        Task<List<TaskItem>> GetTasksForTodayAsync();

        Task<List<TaskItem>> GetTasksForThisWeekAsync();

        Task<List<TaskItem>> GetTasksByDateRangeAsync(DateTime from, DateTime to);

        Task<List<TaskItem>> GetTasksOrderedByDateAsync();
        Task<List<TaskItem>> GetTasksByStateAsync(State state);
        Task UpdateTaskAsync(TaskItem task);
        Task<TaskItem> GetTaskByIdAsync(int taskId);
        Task<List<TaskItem>> GetAllTasksByIDAsync(string id);



    }
}