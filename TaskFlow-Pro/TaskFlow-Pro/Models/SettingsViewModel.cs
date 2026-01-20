namespace TaskFlow_Pro.Models
{
    public class SettingsViewModel
    {
        // Account
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";

        // Preferences
        public string Theme { get; set; } = "Light"; // Light | Dark
        public string DefaultCalendarView { get; set; } = "Month"; // Month | Week | Day
        public bool CompactCalendarMode { get; set; }

        // Notifications
        public bool EmailNotifications { get; set; }
        public bool TaskAssignmentAlerts { get; set; }
        public bool DueDateReminders { get; set; }

        // Team (read-only display)
        public string? TeamName { get; set; }
    }
}
