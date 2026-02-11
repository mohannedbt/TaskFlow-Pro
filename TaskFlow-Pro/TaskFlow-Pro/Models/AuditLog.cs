namespace TaskFlow_Pro.Models
{
    public enum AuditActionType
    {
        TaskCreated,
        TaskAssigned,
        TaskCompleted,
        TaskReopened,
        TeamCreated,
        TeamJoined,
        InviteSent,
        InviteAccepted,
        WorkspaceUpdated,
        UserRoleChanged
    }

    public class AuditLog
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }

        public string? TeamId { get; set; } // if your Team PK is string; otherwise int?
        public int? TaskId { get; set; }

        public string ActorUserId { get; set; } = default!;
        public string ActorName { get; set; } = default!;  // snapshot (so logs survive name changes)

        public AuditActionType ActionType { get; set; }

        public string Title { get; set; } = default!;
        public string? Details { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Optional: for deep-linking in UI
        public string? Url { get; set; }
    }
}
