using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow_Pro.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<TaskItem>  Tasks { get; set; }
    public DbSet<Comment>  Comments { get; set; }
    public DbSet<Team>   Teams { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options){}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // ===== Team ↔ Members =====
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(u => u.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

// ===== Team ↔ Leader =====
        builder.Entity<Team>()
            .HasOne(t => t.Leader)
            .WithMany()
            .HasForeignKey(t => t.LeaderId)
            .OnDelete(DeleteBehavior.Restrict);

    }

}