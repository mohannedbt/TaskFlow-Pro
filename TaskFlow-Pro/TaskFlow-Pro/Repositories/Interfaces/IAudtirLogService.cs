using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            int workspaceId,
            string actorUserId,
            string actorName,
            AuditActionType type,
            string title,
            string? details = null,
            int? taskId = null,
            string? teamId = null,
            string? url = null
        );
    }
}
