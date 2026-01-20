using Microsoft.AspNetCore.Identity;

namespace TaskFlow_Pro.Models;

public class ApplicationUser : IdentityUser
{
    
    // FK to Team (optional membership)
    public int? TeamId { get; set; }

    // Navigation
    public Team? Team { get; set; }
    public ICollection<TaskUserProgress> TaskProgresses { get; set; } = new List<TaskUserProgress>();
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Theme { get; set; } = "Light"; // Light | Dark
    public string DefaultCalendarView { get; set; } = "Month"; // Month | Week | Day
    public bool CompactCalendarMode { get; set; }

    // Notifications
    public bool EmailNotifications { get; set; }
    public bool TaskAssignmentAlerts { get; set; }
    public bool DueDateReminders { get; set; }

}