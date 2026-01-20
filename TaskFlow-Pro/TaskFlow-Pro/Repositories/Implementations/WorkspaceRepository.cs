using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;

namespace TaskFlow_Pro.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly ApplicationDbContext _db;

    public WorkspaceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Workspace?> GetByIdAsync(int id)
    {
        return await _db.Workspaces
            .Include(w => w.Invites)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workspace?> GetByInviteCodeAsync(string code)
    {
        return await _db.WorkspaceInvites
            .Include(i => i.Workspace)
            .Where(i => i.Code == code)
            .Select(i => i.Workspace)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Workspace workspace)
    {
        await _db.Workspaces.AddAsync(workspace);
    }

    public async Task AddInviteAsync(WorkspaceInvite invite)
    {
        await _db.WorkspaceInvites.AddAsync(invite);
    }

    public async Task<int> CountMembersAsync(int workspaceId)
    {
        // ApplicationUser is in Identity DbSet
        return await _db.Users.CountAsync(u => u.WorkspaceId == workspaceId);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}