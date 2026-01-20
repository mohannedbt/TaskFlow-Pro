using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces;

public interface IWorkspaceService
{
    Task<Workspace> CreateWorkspaceAsync(string name, int maxMembers, string emailPattern);

    Task<bool> EmailMatchesWorkspaceAsync(int workspaceId, string email);
    Task<bool> IsWorkspaceFullAsync(int workspaceId);

    Task<WorkspaceInvite> CreateInviteAsync(
        int workspaceId,
        string roleToGrant,        // "Admin" or "Member"
        TimeSpan ttl,
        string? emailLock = null   // optional
    );

    Task<WorkspaceInvite?> ValidateInviteAsync(string code, string email);
    Task MarkInviteUsedAsync(int inviteId);
}