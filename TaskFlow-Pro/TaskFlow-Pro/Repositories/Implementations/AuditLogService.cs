using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;

        public AuditLogService(IAuditLogRepository repo) => _repo = repo;

        public Task LogAsync(int workspaceId, string actorUserId, string actorName,
            AuditActionType type, string title, string? details = null,
            int? taskId = null, string? teamId = null, string? url = null)
        {
            var log = new AuditLog
            {
                WorkspaceId = workspaceId,
                ActorUserId = actorUserId,
                ActorName = actorName,
                ActionType = type,
                Title = title,
                Details = details,
                TaskId = taskId,
                TeamId = teamId,
                Url = url,
                CreatedAtUtc = DateTime.UtcNow
            };

            return _repo.AddAsync(log);
        }
    }
}
