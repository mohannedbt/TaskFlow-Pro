using TaskFlow_Pro.Models;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Creator
    public string CreatedById { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;

    // Team
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // ✅ MANY-TO-MANY USERS
    public ICollection<ApplicationUser> AssignedUsers { get; set; }
        = new List<ApplicationUser>();
    public ICollection<TaskUserProgress> UserProgresses { get; set; } = new List<TaskUserProgress>();


    public State State { get; set; } = State.NotAssigned;
}