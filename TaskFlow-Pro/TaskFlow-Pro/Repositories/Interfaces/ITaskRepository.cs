using TaskFlow_Pro.Models;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);

    Task<TaskItem?> GetByIdAsync(int taskId);
    Task<TaskItem?> GetByIdInWorkspaceAsync(int taskId, int workspaceId);

    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskItem>> GetAllInWorkspaceAsync(int workspaceId);

    Task<List<TaskItem>> GetAllByCreator(string userId);
    Task<List<TaskItem>> GetAllByCreatorInWorkspaceAsync(string userId, int workspaceId);

    Task<List<TaskItem>> GetAllByTeamInWorkspaceAsync(int teamId, int workspaceId);

    Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId);
    Task<List<TaskItem>> GetTasksAssignedToUserInWorkspaceAsync(string userId, int workspaceId);

    Task<TaskItem?> GetTaskWithProgressAsync(int taskId);
    Task<TaskUserProgress?> GetProgressAsync(int taskId, string userId);
    Task AddProgressAsync(TaskUserProgress progress);
    Task SaveAsync();

    Task<State?> GetMyStateAsync(int taskId, string userId);
}