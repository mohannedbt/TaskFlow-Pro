using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models;

public class SettingsViewModel
{
    // ===== Account =====
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // ===== Preferences =====
    public string Theme { get; set; } = "Light"; // Light | Dark | System
    public string CalendarView { get; set; } = "Month"; // Month | Week
    public bool CompactCalendar { get; set; }

    // ===== Notifications =====
    public bool EmailNotifications { get; set; }
    public bool TaskAssignedNotification { get; set; }
    public bool DueDateReminder { get; set; }

    // ===== Team (read-only display) =====
    public string? TeamName { get; set; }
}