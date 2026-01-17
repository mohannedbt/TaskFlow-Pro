using Microsoft.AspNetCore.Identity;

namespace TaskFlow_Pro.Models;

public class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    // ===== Leader (one user leads a team) =====
    public string LeaderId { get; set; } = null!;
    public ApplicationUser Leader { get; set; } = null!;

    // ===== Members (one team has many users) =====
    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
}