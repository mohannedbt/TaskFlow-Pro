using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _repo;
    private readonly ApplicationDbContext _db; // used to fetch invite rows cleanly

    public WorkspaceService(IWorkspaceRepository repo, ApplicationDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<Workspace> CreateWorkspaceAsync(string name, int maxMembers, string emailPattern)
    {
        name = name.Trim();
        emailPattern = emailPattern.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workspace name is required.");

        if (maxMembers < 1)
            throw new ArgumentException("Max members must be >= 1.");

        // Validate regex early
        if (!IsValidRegex(emailPattern))
            throw new ArgumentException("EmailPattern is not a valid regex.");

        var workspace = new Workspace
        {
            Name = name,
            MaxMembers = maxMembers,
            EmailPattern = emailPattern
        };

        await _repo.AddAsync(workspace);
        await _repo.SaveAsync();

        return workspace;
    }

    public async Task<bool> EmailMatchesWorkspaceAsync(int workspaceId, string email)
    {
        var workspace = await _repo.GetByIdAsync(workspaceId);
        if (workspace == null) return false;

        return Regex.IsMatch(email, workspace.EmailPattern, RegexOptions.IgnoreCase);
    }

    public async Task<bool> IsWorkspaceFullAsync(int workspaceId)
    {
        var workspace = await _repo.GetByIdAsync(workspaceId);
        if (workspace == null) return true;

        var count = await _repo.CountMembersAsync(workspaceId);
        return count >= workspace.MaxMembers;
    }

    public async Task<WorkspaceInvite> CreateInviteAsync(int workspaceId, string roleToGrant, TimeSpan ttl, string? emailLock = null)
    {
        var workspace = await _repo.GetByIdAsync(workspaceId);
        if (workspace == null) throw new Exception("Workspace not found.");

        roleToGrant = NormalizeRole(roleToGrant);

        var invite = new WorkspaceInvite
        {
            WorkspaceId = workspaceId,
            RoleToGrant = roleToGrant,
            Code = GenerateCode(24),
            ExpiresAt = DateTime.UtcNow.Add(ttl),
            Email = string.IsNullOrWhiteSpace(emailLock) ? null : emailLock.Trim(),
            Used = false
        };

        await _repo.AddInviteAsync(invite);
        await _repo.SaveAsync();

        return invite;
    }

    // Returns the invite row if valid, else null
    public async Task<WorkspaceInvite?> ValidateInviteAsync(string code, string email)
    {
        code = code.Trim();

        var invite = await _db.WorkspaceInvites
            .Include(i => i.Workspace)
            .FirstOrDefaultAsync(i => i.Code == code);

        if (invite == null) return null;
        if (invite.Used) return null;
        if (invite.ExpiresAt < DateTime.UtcNow) return null;

        // Optional email lock
        if (!string.IsNullOrWhiteSpace(invite.Email) &&
            !string.Equals(invite.Email, email, StringComparison.OrdinalIgnoreCase))
            return null;

        // Pattern check
        if (!Regex.IsMatch(email, invite.Workspace.EmailPattern, RegexOptions.IgnoreCase))
            return null;

        // Capacity check
        var members = await _repo.CountMembersAsync(invite.WorkspaceId);
        if (members >= invite.Workspace.MaxMembers)
            return null;

        // Ensure role is safe
        invite.RoleToGrant = NormalizeRole(invite.RoleToGrant);

        return invite;
    }

    public async Task MarkInviteUsedAsync(int inviteId)
    {
        var invite = await _db.WorkspaceInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
        if (invite == null) return;

        invite.Used = true;
        await _db.SaveChangesAsync();
    }

    // ---------------------
    // Helpers
    // ---------------------
    private static bool IsValidRegex(string pattern)
    {
        try { _ = new Regex(pattern); return true; }
        catch { return false; }
    }

    private static string NormalizeRole(string role)
    {
        // Owner must NOT be granted by invites (only by creating workspace)
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)) return "Admin";
        return "Member";
    }

    private static string GenerateCode(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rng = Random.Shared;

        var buffer = new char[length];
        for (int i = 0; i < length; i++)
            buffer[i] = chars[rng.Next(chars.Length)];

        return new string(buffer);
    }
}
