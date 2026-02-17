// RequirementService/Data/RequirementDbContext.cs
using Microsoft.EntityFrameworkCore;
using RequirementService.Models;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Data;

[ExcludeFromCodeCoverage]

public class RequirementDbContext : DbContext
{
    public RequirementDbContext(DbContextOptions<RequirementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Requirement> Requirements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table name explicitly set
        modelBuilder.Entity<Requirement>()
            .ToTable("Requirements");

        // Indexes
        modelBuilder.Entity<Requirement>()
            .HasIndex(r => r.Project)
            .HasDatabaseName("IX_Requirements_Project");

        modelBuilder.Entity<Requirement>()
            .HasIndex(r => r.SkillsNeeded)
            .HasDatabaseName("IX_Requirements_Skills");

        modelBuilder.Entity<Requirement>()
            .HasIndex(r => r.AvailabilityWindow)
            .HasDatabaseName("IX_Requirements_Availability");

        // Default values
        modelBuilder.Entity<Requirement>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}