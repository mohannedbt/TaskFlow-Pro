using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<List<AuditLog>> GetByWorkspaceAsync(int workspaceId, int take = 50);
        Task<List<AuditLog>> GetByTeamAsync(int workspaceId, int? teamId, int take = 50);
    }
}
