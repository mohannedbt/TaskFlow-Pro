using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow_Pro.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<TaskItem>  Tasks { get; set; }
    public DbSet<Comment>  Comments { get; set; }
    public DbSet<Team>   Teams { get; set; }
    public DbSet<TaskUserProgress> TaskUserProgresses { get; set; } = null!;
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceInvite>  WorkspaceInvites { get; set; }

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
        builder.Entity<TaskUserProgress>()
            .HasIndex(p => new { p.TaskItemId, p.UserId })
            .IsUnique(); // ✅ one row per (Task, User)

        builder.Entity<TaskUserProgress>()
            .HasOne(p => p.TaskItem)
            .WithMany(t => t.UserProgresses)
            .HasForeignKey(p => p.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskUserProgress>()
            .HasOne(p => p.User)
            .WithMany(u => u.TaskProgresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<WorkspaceInvite>()
            .HasIndex(i => i.Code)
            .IsUnique();
        // ===== Workspace ↔ Users =====
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Workspace)
            .WithMany(w => w.Users)
            .HasForeignKey(u => u.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

// ===== Workspace ↔ Teams =====
        builder.Entity<Team>()
            .HasOne(t => t.Workspace)
            .WithMany(w => w.Teams)
            .HasForeignKey(t => t.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

// ===== Workspace ↔ Tasks =====
        builder.Entity<TaskItem>()
            .HasOne(t => t.Workspace)
            .WithMany(w => w.Tasks)
            .HasForeignKey(t => t.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

// ===== Workspace ↔ TaskUserProgress =====
        builder.Entity<TaskUserProgress>()
            .HasOne(p => p.Workspace)
            .WithMany(w => w.TaskUserProgresses)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

    }

}