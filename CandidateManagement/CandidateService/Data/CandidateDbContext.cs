using CandidateService.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Data
{
    [ExcludeFromCodeCoverage]
    public class CandidateDbContext : DbContext
    {
        public CandidateDbContext(DbContextOptions<CandidateDbContext> options)
            : base(options)
        {
        }

        public DbSet<Candidate> Candidates { get; set; } = null!;
        public DbSet<CandidateStaging> CandidateStaging { get; set; } = null!;
        public DbSet<Interview> Interviews { get; set; } = null!;
        public DbSet<Requirement> Requirements { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Feedback> Feedback { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candidate>()
                .HasIndex(c => new { c.MailId, c.SkillSet, c.AvailabilityDate })
                .IsUnique();
        }
    }
}
