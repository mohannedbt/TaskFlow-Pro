using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Repositories.Interfaces;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(int id);
    Task<Workspace?> GetByInviteCodeAsync(string code);

    Task AddAsync(Workspace workspace);
    Task AddInviteAsync(WorkspaceInvite invite);

    Task<int> CountMembersAsync(int workspaceId);

    Task SaveAsync();
}