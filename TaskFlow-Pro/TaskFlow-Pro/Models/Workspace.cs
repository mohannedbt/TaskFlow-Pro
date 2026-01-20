namespace TaskFlow_Pro.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    // Safety: limit the number of members
    public int MaxMembers { get; set; } = 50;
    // email domain identifier like "enterpriseX.com"
    public string EmailPattern { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<WorkspaceInvite> Invites { get; set; }
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskUserProgress> TaskUserProgresses { get; set; } = new List<TaskUserProgress>();

}
