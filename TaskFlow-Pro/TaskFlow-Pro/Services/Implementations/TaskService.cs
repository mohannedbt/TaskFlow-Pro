using Microsoft.AspNetCore.Identity;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services.Implementations;
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ITeamService _teamService;

    public TaskService(
        ITaskRepository repo,
        ITeamService teamService)
    {
        _repo = repo;
        _teamService = teamService;
    }

    public async Task<TaskItem> CreateTaskAsync(
        string title,
        string description,
        string creatorUserId,
        DateTime startDate,
        DateTime endDate,
        int workspaceId)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            CreatedById = creatorUserId,
            State = State.NotAssigned,
            WorkspaceId = workspaceId // ✅ critical
        };

        await _repo.AddAsync(task);
        return task;
    }





    public async Task ChangeTaskStateAsync(
        int taskId,
        State newState,
        string userId)
    {
        var task = await _repo.GetByIdAsync(taskId);
        if (task == null) return;

        // optional: permission checks here

        task.State = newState;
        await _repo.UpdateAsync(task);
    }

    public async Task<List<TaskItem>> GetAllTasksByCreatorIDAsync(string id)
        => await _repo.GetAllByCreator(id);

    public async Task<List<TaskItem>> GetAllTasksByAssignedIdAsync(string id)
        => await _repo.GetTasksAssignedToUserAsync(id);

    public async Task<List<TaskItem>> GetAllTasksByTeamIdAsync(int teamId)
    {
        return await _repo.GetAllAsync()
            .ContinueWith(t => t.Result.Where(x => x.TeamId == teamId).ToList());
    }

    public async Task<TaskItem> GetAllTasksByTaskIdAsync(int taskId)
    {
        return await _repo.GetByIdAsync(taskId);
    }

    public async Task<List<TaskItem>> GetAllTasks()
    {
        return await _repo.GetAllAsync();
    }
    public Task<List<TaskItem>> ApplyFiltersAsync(List<TaskItem> tasks, TaskFilterViewModel f)
    {
        IEnumerable<TaskItem> q = tasks;

        // Search
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim();
            q = q.Where(t =>
                (t.Title != null && t.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (t.Description != null && t.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        // State
        if (f.State.HasValue)
            q = q.Where(t => t.State == f.State.Value);

        // Only unassigned (TeamId == null)
        if (f.OnlyUnassigned)
            q = q.Where(t => t.TeamId == null);

        // Date range
        if (f.Range == "today")
        {
            var today = DateTime.Today;
            q = q.Where(t => t.StartDate.Date == today);
        }
        else if (f.Range == "week")
        {
            var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var end = start.AddDays(7);
            q = q.Where(t => t.StartDate >= start && t.StartDate < end);
        }
        else if (f.Range == "custom" && f.From.HasValue && f.To.HasValue)
        {
            q = q.Where(t => t.StartDate >= f.From.Value && t.EndDate <= f.To.Value);
        }

        // Sorting
        q = f.Sort switch
        {
            "start_asc" => q.OrderBy(t => t.StartDate),
            "end_asc"   => q.OrderBy(t => t.EndDate),
            "end_desc"  => q.OrderByDescending(t => t.EndDate),
            _           => q.OrderByDescending(t => t.StartDate), // start_desc default
        };

        return Task.FromResult(q.ToList());
    }
    public async Task SetMyProgressStateAsync(int taskId, string userId, int workspaceId, State newState)
    {
        var task = await _repo.GetTaskWithProgressAsync(taskId);
        if (task == null) throw new Exception("Task not found.");

        // ✅ Safety: prevent cross-workspace access
        if (task.WorkspaceId != workspaceId)
            throw new UnauthorizedAccessException("Task is not in your workspace.");

        var progress = await _repo.GetProgressAsync(taskId, userId);

        if (progress == null)
        {
            progress = new TaskUserProgress
            {
                TaskItemId = taskId,
                UserId = userId,
                WorkspaceId = workspaceId,   // ✅ ALWAYS SET
                State = State.Ongoing
            };

            await _repo.AddProgressAsync(progress);
        }

        progress.State = newState;
        progress.CompletedAt = (newState == State.Completed) ? DateTime.UtcNow : null;

        await _repo.SaveAsync();
        await RecomputeTaskStateAsync(taskId);
    }

    public async Task AddProgressAsync(TaskUserProgress progress)
    {
        await _repo.AddProgressAsync(progress);
    }



    public async Task RecomputeTaskStateAsync(int taskId)
    {
        var task = await _repo.GetTaskWithProgressAsync(taskId);
        if (task == null) return;

        // if no team => NotAssigned
        if (!task.TeamId.HasValue)
        {
            task.State = State.NotAssigned;
            await _repo.SaveAsync();
            return;
        }

        var states = task.UserProgresses.Select(p => p.State).ToList();

        // no assignees/progress rows => leave it ongoing (or keep old)
        if (states.Count == 0)
        {
            if (task.State == State.NotAssigned) task.State = State.Ongoing;
            await _repo.SaveAsync();
            return;
        }

        bool allCompleted = states.All(s => s == State.Completed);
        bool anyOngoing = states.Any(s => s == State.Ongoing);
        bool anyInterrupted = states.Any(s => s == State.Interrupted);

        task.State =
            allCompleted ? State.Completed :
            anyOngoing ? State.Ongoing :
            anyInterrupted ? State.Interrupted :
            State.Ongoing;

        await _repo.SaveAsync();
    }

    public Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId)
        => _repo.GetTasksAssignedToUserAsync(userId);

    public Task<State?> GetMyStateAsync(int taskId, string userId)
        => _repo.GetMyStateAsync(taskId, userId);
    public Task<List<TaskItem>> GetAllTasksByCreatorIDAsync(string id, int workspaceId)
        => _repo.GetAllByCreatorInWorkspaceAsync(id, workspaceId);

    public Task<List<TaskItem>> GetAllTasksByAssignedIdAsync(string id, int workspaceId)
        => _repo.GetTasksAssignedToUserInWorkspaceAsync(id, workspaceId);

    public Task<List<TaskItem>> GetAllTasksByTeamIdAsync(int teamId, int workspaceId)
        => _repo.GetAllByTeamInWorkspaceAsync(teamId, workspaceId);

    public Task<TaskItem?> GetAllTasksByTaskIdAsync(int taskId, int workspaceId)
        => _repo.GetByIdInWorkspaceAsync(taskId, workspaceId);

    public Task<List<TaskItem>> GetAllTasks(int workspaceId)
        => _repo.GetAllInWorkspaceAsync(workspaceId);

    public Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId, int workspaceId)
        => _repo.GetTasksAssignedToUserInWorkspaceAsync(userId, workspaceId);
    
    public async Task AssignTaskToTeamAsync(int taskId, int teamId, int workspaceId)
    {
        // Load task WITH progresses so we can avoid duplicates
        var task = await _repo.GetTaskWithProgressAsync(taskId);
        if (task == null) throw new Exception("Task not found.");

        // HARD SAFETY: task must be in same workspace
        // If you have old tasks created before workspace support, they might be 0.
        // Patch them once:
        if (task.WorkspaceId == 0)
            task.WorkspaceId = workspaceId;

        if (task.WorkspaceId != workspaceId)
            throw new UnauthorizedAccessException("Cross-workspace assign blocked.");

        // Team members inside same workspace
        var members = await _teamService.GetTeamMembersAsync(teamId, workspaceId);

        task.TeamId = teamId;
        task.State = State.Ongoing;

        foreach (var member in members)
        {
            bool exists = task.UserProgresses.Any(p => p.UserId == member.Id);
            if (exists) continue;

            task.UserProgresses.Add(new TaskUserProgress
            {
                TaskItemId = task.Id,
                UserId = member.Id,
                WorkspaceId = workspaceId,   // ✅ ALWAYS USE workspaceId HERE
                State = State.Ongoing
            });
        }

        await _repo.UpdateAsync(task); // Save everything
    }


    public async Task SaveAsync()
    {
        await _repo.SaveAsync();
    }
}

