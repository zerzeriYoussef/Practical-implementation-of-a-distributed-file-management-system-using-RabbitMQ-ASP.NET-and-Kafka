using Microsoft.EntityFrameworkCore;
using MvP.Domain.Entities.Teams;

namespace MvP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasIndex(t => t.JoinCode)
            .IsUnique();

        modelBuilder.Entity<TeamMember>()
            .HasIndex(m => new { m.TeamId, m.UserId })
            .IsUnique();

        modelBuilder.Entity<TeamMember>()
            .HasOne(m => m.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamId);
    }
}