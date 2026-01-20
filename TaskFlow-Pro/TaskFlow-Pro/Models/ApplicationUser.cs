using Microsoft.AspNetCore.Identity;

namespace TaskFlow_Pro.Models;

public class ApplicationUser : IdentityUser
{
    
    // FK to Team (optional membership)
    public int? TeamId { get; set; }

    // Navigation
    public Team? Team { get; set; }
    public ICollection<TaskUserProgress> TaskProgresses { get; set; } = new List<TaskUserProgress>();

}