using TaskFlow_Pro.Models;

public interface ITaskService
{
    Task<TaskItem> CreateTaskAsync(
        string title,
        string description,
        string creatorUserId,
        DateTime startDate,
        DateTime endDate,
        int workspaceId);

    Task AssignTaskToTeamAsync(int taskId, int teamId, int workspaceId);

    Task ChangeTaskStateAsync(int taskId, State newState, string userId);
    Task SetMyProgressStateAsync(int taskId, string userId, int workspaceId, State newState);

    Task<List<TaskItem>> GetAllTasksByCreatorIDAsync(string userId, int workspaceId);
    Task<List<TaskItem>> GetAllTasksByAssignedIdAsync(string userId, int workspaceId);
    Task<List<TaskItem>> GetAllTasksByTeamIdAsync(int teamId, int workspaceId);
    Task<TaskItem?> GetAllTasksByTaskIdAsync(int taskId, int workspaceId);

    Task<List<TaskItem>> GetAllTasks(int workspaceId);

    Task<List<TaskItem>> ApplyFiltersAsync(List<TaskItem> tasks, TaskFilterViewModel f);

    Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId, int workspaceId);
    Task<State?> GetMyStateAsync(int taskId, string userId);
    Task AddProgressAsync(TaskUserProgress progress);
    Task SaveAsync();

}