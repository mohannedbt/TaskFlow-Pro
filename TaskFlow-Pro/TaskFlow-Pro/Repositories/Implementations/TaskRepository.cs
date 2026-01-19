using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;

namespace TaskFlow_Pro.Repositories.Implementations;

public class TaskRepository: ITaskRepository
{
    public ApplicationDbContext _context { get; set; }

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(TaskItem task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
         _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        var list=await _context.Tasks.ToListAsync();
        return list;
    }

    public async Task<List<TaskItem>> GetAllByCreator(string id)
    {
        var list= await _context.Tasks.ToListAsync();
        var query= from t in list where t.CreatedById == id select t;
        return query.ToList();
    }

    public async Task<TaskItem?> GetByIdAsync(int taskId)
    {
        var data =await _context.Tasks.ToListAsync();
        var query=from t in data where  t.Id == taskId select t;
        return query.FirstOrDefault();
    }

    public async Task<List<TaskItem>> GetRecentTasksAsync(int days)
    {
        var data = await _context.Tasks.ToListAsync();
        var query = from t in data where t.StartDate > DateTime.Now.AddDays(-days) select t;
        return query.ToList();
    }

    public async Task<Dictionary<State, int>> GetTaskCountByStateAsync()
    {
        var data = await _context.Tasks.ToListAsync();
        var query=from t in data group t.State by t.State;
        return query.ToDictionary(x=>x.Key,x=>x.Count());
    }

    public async Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId)
    {
        var data = await _context.Tasks.Include(u=>u.AssignedTo).ToListAsync();
        var query = from t in data where t.AssignedTo.Id==userId select t;
        return query.ToList();
    }

    public async Task<List<TaskItem>> GetTasksByStateAsync(State state)
    {
        var data = await _context.Tasks.ToListAsync();
        var query = from t in data where t.State == state select t;
        return query.ToList();
    }
    

    public async Task<List<TaskItem>> GetTasksOrderedByDateAsync()
    {
        var data = await _context.Tasks.ToListAsync();
        var query = from t in data orderby t.StartDate descending select t;
        return query.ToList();
    }

    public async Task<List<TaskItem>> GetTasksWaitingForReviewAsync()
    {
        var data = await _context.Tasks.Include(t=>t.Comments).ToListAsync();
        var query = from t in data where t.Comments.Count==0 orderby t.EndDate descending select t;
        return query.ToList();
    }

    public async Task<List<TaskItem>> SearchTasksAsync(string keyword)
    {
        var data = await _context.Tasks.ToListAsync();
        var query=from t in data where t.Description.Contains(keyword)|| t.Title.Contains(keyword) select t;
        return query.ToList();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }
}
