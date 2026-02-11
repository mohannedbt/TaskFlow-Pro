using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;

namespace TaskFlow_Pro.Repositories.Implementations
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _db;

        public AuditLogRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(AuditLog log)
        {
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public Task<List<AuditLog>> GetByWorkspaceAsync(int workspaceId, int take = 50)
        {
            return _db.AuditLogs
                .Where(x => x.WorkspaceId == workspaceId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToListAsync();
        }

        public Task<List<AuditLog>> GetByTeamAsync(int workspaceId, int? teamId, int take = 50)
        {
            if (teamId == null)
                return Task.FromResult(new List<AuditLog>());

            return _db.AuditLogs
                .Where(x => x.WorkspaceId == workspaceId && x.TeamId == teamId.ToString())
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToListAsync();
        }
    }
}
