using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models;

public class TaskViewModel
{
    [Required]
    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    // Teams that the current user is allowed to assign tasks to
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public TaskFilterViewModel Filters { get; set; } = new();


    // Bulk
    public List<int> TaskIds { get; set; } = new();

    public State? Action { get; set; }
    public Dictionary<int, State?> MyStates { get; set; } = new();

    
}