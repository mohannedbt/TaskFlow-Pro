namespace TaskFlow_Pro.Models;

public class WorkspaceInvite
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public string Code { get; set; } = null!;
    public string RoleToGrant { get; set; } = "Member"; // "Admin" or "Member"
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; } = false;

    // optional: restrict invite to specific email
    public string? Email { get; set; }
}
