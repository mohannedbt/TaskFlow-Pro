using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int? TeamId { get; set; }

        public string? TeamName { get; set; }

        public List<Team> AvailableTeams { get; set; } = new();
    }
}