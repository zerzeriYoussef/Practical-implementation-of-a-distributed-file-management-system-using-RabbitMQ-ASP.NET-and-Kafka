using Microsoft.EntityFrameworkCore;
using MvP.Domain.Entities.Storage;
using MvP.Domain.Entities.Teams;

namespace MvP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

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

        modelBuilder.Entity<StoredFile>()
            .HasIndex(f => new { f.TeamId, f.DeletedAt });

        modelBuilder.Entity<StoredFile>()
            .HasIndex(f => f.OwnerUserId);

        modelBuilder.Entity<StoredFile>()
            .HasIndex(f => f.UploadedByUserId);

        modelBuilder.Entity<StoredFile>()
            .HasIndex(f => f.S3Key)
            .IsUnique();

        modelBuilder.Entity<StoredFile>()
            .HasOne(f => f.Team)
            .WithMany(t => t.Files)
            .HasForeignKey(f => f.TeamId);
    }
}
