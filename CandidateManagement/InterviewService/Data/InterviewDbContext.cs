using InterviewService.Models;
using InterviewService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Data;

[ExcludeFromCodeCoverage]
public class InterviewDbContext : DbContext
{

    public InterviewDbContext(DbContextOptions<InterviewDbContext> options)
        : base(options)
    {
    }

    public DbSet<Interview> Interviews { get; set; } = null!;


    public DbSet<Feedback> Feedbacks { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // === INDEXES ===
        // For 6-month rule lookups
        modelBuilder.Entity<Interview>()
            .HasIndex(i => new { i.CandidateId, i.Project, i.InterviewDate })
            .HasDatabaseName("IX_Interviews_Candidate_Project_Date");

        // For candidate history
        modelBuilder.Entity<Interview>()
            .HasIndex(i => i.CandidateId)
            .HasDatabaseName("IX_Interviews_CandidateId");

        // For requirement matching
        modelBuilder.Entity<Interview>()
            .HasIndex(i => i.RequirementId)
            .HasDatabaseName("IX_Interviews_RequirementId");

        // For date range queries
        modelBuilder.Entity<Interview>()
            .HasIndex(i => i.InterviewDate)
            .HasDatabaseName("IX_Interviews_InterviewDate");

        // For feedback lookups
        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.InterviewId)
            .HasDatabaseName("IX_Feedbacks_InterviewId");

        // === RELATIONSHIPS ===
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Interview)
            .WithMany(i => i.Feedbacks)
            .HasForeignKey(f => f.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // === DEFAULT VALUES ===
        modelBuilder.Entity<Interview>()
            .Property(i => i.FinalOutcome)
            .HasDefaultValue(InterviewOutcome.Pending);

        modelBuilder.Entity<Interview>()
            .Property(i => i.DecisionMaker)
            .HasDefaultValue(DecisionMaker.NotApplicable);

        modelBuilder.Entity<Interview>()
            .Property(i => i.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<Feedback>()
            .Property(f => f.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // === CONSTRAINTS ===
        modelBuilder.Entity<Interview>()
            .ToTable(t => t.HasCheckConstraint("CK_Interview_Level", "[Level] IN (1, 2)"));

        modelBuilder.Entity<Interview>()
            .ToTable(t => t.HasCheckConstraint("CK_Interview_Outcome", "[FinalOutcome] IN (0, 1, 2)"));

        modelBuilder.Entity<Interview>()
            .ToTable(t => t.HasCheckConstraint("CK_Interview_DecisionMaker", "[DecisionMaker] IN (0, 1, 2, 3)"));
    }
}