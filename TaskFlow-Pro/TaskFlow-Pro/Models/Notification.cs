using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Models
{
    public enum NotificationType
    {
        TaskAssigned,
        TaskCompleted,
        InviteCreated,
        InviteAccepted,
        DeadlineSoon,
        System
    }

    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public NotificationType Type { get; set; } = NotificationType.System;

        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Context (optionnel)
        public int WorkspaceId { get; set; }
        public int? TaskItemId { get; set; }
        public int? TeamId { get; set; }

        public TaskItem? TaskItem { get; set; }
        public Team? Team { get; set; }
        public Workspace Workspace { get; set; } = null!;
    }
}
