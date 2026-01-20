using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces;

public interface ITaskService
{
    // =========================
    // CREATE
    // =========================
    Task<TaskItem> CreateTaskAsync(
        string title,
        string description,
        string creatorUserId,
        DateTime startDate,
        DateTime endDate);
    

    // =========================
    // ASSIGNMENT
    // =========================
    Task AssignTaskToTeamAsync(int taskId, int teamId);

    // =========================
    // STATE
    // =========================
    Task ChangeTaskStateAsync(
        int taskId,
        State newState,
        string userId);

    // =========================
    // QUERIES
    // =========================
    Task<List<TaskItem>> GetAllTasksByCreatorIDAsync(string userId);
    Task<List<TaskItem>> GetAllTasksByAssignedIdAsync(string userId);
    Task<List<TaskItem>> GetAllTasksByTeamIdAsync(int teamId);
    Task<TaskItem> GetAllTasksByTaskIdAsync(int taskId);
    Task<List<TaskItem>> GetAllTasks();
    Task<List<TaskItem>> ApplyFiltersAsync(List<TaskItem> tasks, TaskFilterViewModel f);
    Task SetMyProgressStateAsync(int taskId, string userId, State newState);
    Task RecomputeTaskStateAsync(int taskId);
    Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId);

    Task<State?> GetMyStateAsync(int taskId, string userId);







}