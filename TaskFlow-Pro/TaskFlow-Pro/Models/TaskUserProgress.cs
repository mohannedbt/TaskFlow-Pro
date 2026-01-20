using TaskFlow_Pro.Models;

public class TaskUserProgress
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public State State { get; set; } = State.Ongoing;

    public DateTime? CompletedAt { get; set; }
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}