using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;

namespace TaskFlow_Pro.Repositories.Implementations;
public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int taskId)
    {
        return await _context.Tasks
            .Include(t => t.UserProgresses)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetAllByCreator(string userId)
    {
        return await _context.Tasks
            .Where(t => t.CreatedById == userId)
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }



    public async Task<List<TaskItem>> GetTasksByStateAsync(State state)
    {
        return await _context.Tasks
            .Where(t => t.State == state)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetTasksOrderedByDateAsync()
    {
        return await _context.Tasks
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }

    public async Task<List<TaskUserProgress>> GetAllTaskUserProgressAsync()
    {
        return await _context.TaskUserProgresses.ToListAsync();
    }
    public async Task<TaskItem?> GetTaskWithProgressAsync(int taskId)
    {
        return await _context.Tasks
            .Include(t => t.UserProgresses)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task<TaskUserProgress?> GetProgressAsync(int taskId, string userId)
    {
        return await _context.TaskUserProgresses
            .FirstOrDefaultAsync(p => p.TaskItemId == taskId && p.UserId == userId);
    }

    public async Task AddProgressAsync(TaskUserProgress progress)
    {
        await _context.TaskUserProgresses.AddAsync(progress);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId)
    {
        // tasks where this user has a progress row
        return await _context.TaskUserProgresses
            .Where(p => p.UserId == userId)
            .Select(p => p.TaskItem)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<TaskUserProgress>> GetProgressRowsForTaskAsync(int taskId)
    {
        return await _context.TaskUserProgresses
            .Include(p => p.User)
            .Where(p => p.TaskItemId == taskId)
            .ToListAsync();
    }
    public async Task<State?> GetMyStateAsync(int taskId, string userId)
    {
        return await _context.TaskUserProgresses
            .Where(p => p.TaskItemId == taskId && p.UserId == userId)
            .Select(p => (State?)p.State)
            .FirstOrDefaultAsync();
    }
    public async Task<TaskItem?> GetByIdInWorkspaceAsync(int taskId, int workspaceId)
    {
        return await _context.Tasks
            .Include(t => t.UserProgresses)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.WorkspaceId == workspaceId);
    }

    public async Task<List<TaskItem>> GetAllInWorkspaceAsync(int workspaceId)
    {
        return await _context.Tasks
            .Where(t => t.WorkspaceId == workspaceId)
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetAllByCreatorInWorkspaceAsync(string userId, int workspaceId)
    {
        return await _context.Tasks
            .Where(t => t.CreatedById == userId && t.WorkspaceId == workspaceId)
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetAllByTeamInWorkspaceAsync(int teamId, int workspaceId)
    {
        return await _context.Tasks
            .Where(t => t.TeamId == teamId && t.WorkspaceId == workspaceId)
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetTasksAssignedToUserInWorkspaceAsync(string userId, int workspaceId)
    {
        return await _context.TaskUserProgresses
            .Where(p => p.UserId == userId && p.TaskItem.WorkspaceId == workspaceId)
            .Select(p => p.TaskItem)
            .Distinct()
            .Include(t => t.UserProgresses)
            .ToListAsync();
    }


}
