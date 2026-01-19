using Microsoft.AspNetCore.Identity;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services.Implementations;

public class TaskService : ITaskService
{
    public ITaskRepository _Taskrepo;
    public ApplicationDbContext _context;
    public UserManager<ApplicationUser> _userManager;
    
    private static readonly Dictionary<State, HashSet<State>> AllowedTransitions =
        new()
        {
            { State.NotAssigned, new() { State.Ongoing } },

            { State.Ongoing, new()
                {
                    State.Interrupted,
                    State.Completed
                }
            },

            { State.Interrupted, new()
                {
                    State.Ongoing,
                    State.Canceled
                }
            },

            { State.Completed, new() },
            { State.Canceled, new() }
        };


    public TaskService(ITaskRepository repo, ApplicationDbContext context,UserManager<ApplicationUser> userManager)
    {
        _Taskrepo = repo;
        _context = context;
        _userManager=userManager;
        
    }
    public async Task<List<TaskItem>> GetTasksByStateAsync(State state)
        => await _Taskrepo.GetTasksByStateAsync(state);

    public async Task<List<TaskItem>> SearchTasksAsync(string q)
    {
        return await _Taskrepo.SearchTasksAsync(q);
    }

    public async Task<TaskItem> CreateTaskAsync(
        string title,
        string description,
        string creatorUserId,
        DateTime startDate,
        DateTime endDate)
    {
        var item = new TaskItem
        {
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,

            // ✅ ONLY SET THE FK
            CreatedById = creatorUserId,

            State = State.NotAssigned
        };

        await _Taskrepo.AddAsync(item);
        return item;
    }


    public async  Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _Taskrepo.GetAllAsync();
    }

    public async Task<List<TaskItem>> GetAllTasksByIDAsync(string id)
    {
        return await _Taskrepo.GetAllByCreator(id);
    }


public async Task AssignTaskAsync(int taskId, ApplicationUser assignedUser)
{
    TaskItem taskItem = await _Taskrepo.GetByIdAsync(taskId);
    taskItem.AssignedTo=assignedUser;
    await _Taskrepo.UpdateAsync(taskItem);
}

public async Task ChangeTaskStateAsync(int taskId, State newState, string actingUserId)
    {
        TaskItem taskItem= await _Taskrepo.GetByIdAsync(taskId);
        if (AllowedTransitions[taskItem.State].Contains(newState))
        {
            taskItem.State=newState;
            await _Taskrepo.UpdateAsync(taskItem);
        }
    }
    public async Task<List<TaskItem>> GetTasksForTodayAsync()
    {
        return await _Taskrepo.GetRecentTasksAsync(1);
    }

    public async Task<List<TaskItem>> GetTasksForThisWeekAsync()
    {
        return await _Taskrepo.GetRecentTasksAsync(7);
    }

    public async Task<List<TaskItem>> GetTasksByDateRangeAsync(DateTime from, DateTime to)
    {
        var all = await _Taskrepo.GetAllAsync();

        return all
            .Where(t =>
                t.StartDate.Date >= from.Date &&
                t.EndDate.Date <= to.Date)
            .ToList();
    }

    public async Task<List<TaskItem>> GetTasksOrderedByDateAsync()
    {
        return await _Taskrepo.GetTasksOrderedByDateAsync();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
         await _Taskrepo.UpdateAsync(task);
        
    }

    public async Task<TaskItem> GetTaskByIdAsync(int taskId)
    {
        return await _Taskrepo.GetByIdAsync(taskId);
    }
}
    
