using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models;

public class WorkspaceIndexViewModel
{
    // Workspace info
    public int WorkspaceId { get; set; }

    [Required, StringLength(60)]
    public string WorkspaceName { get; set; } = string.Empty;

    [Range(1, 5000)]
    public int MaxMembers { get; set; }

    // stored domain (e.g. "miaw.com")
    [Required, StringLength(120)]
    public string EmailDomain { get; set; } = string.Empty;

    // Lists
    public List<TeamWithMembersVm> Teams { get; set; } = new();
    public List<MemberVm> Members { get; set; } = new();

    // Create team form
    [StringLength(60)]
    public string? NewTeamName { get; set; }

    [StringLength(200)]
    public string? NewTeamDescription { get; set; }

    public int MemberCount { get; set; }
}

public class TeamWithMembersVm
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<MemberVm> Members { get; set; } = new();
}

public class MemberVm
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
}