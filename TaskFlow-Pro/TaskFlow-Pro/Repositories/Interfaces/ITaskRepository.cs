using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Repositories.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);

    Task<TaskItem?> GetByIdAsync(int taskId);

    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskItem>> GetAllByCreator(string creatorId);
    Task<TaskItem?> GetTaskWithProgressAsync(int taskId);
    Task<TaskUserProgress?> GetProgressAsync(int taskId, string userId);
    Task AddProgressAsync(TaskUserProgress progress);
    Task SaveAsync();

    Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId); // modern "MyAssigned"
    Task<List<TaskUserProgress>> GetProgressRowsForTaskAsync(int taskId);
    Task<State?> GetMyStateAsync(int taskId, string userId);

}