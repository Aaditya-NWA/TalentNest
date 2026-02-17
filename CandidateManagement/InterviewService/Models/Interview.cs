using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Models;

[ExcludeFromCodeCoverage]
public class Interview
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CandidateId { get; set; }

    [Required]
    public int RequirementId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Project { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Interviewer { get; set; } = string.Empty;

    [Required]
    public DateTime InterviewDate { get; set; }

    [Required]
    public InterviewLevel Level { get; set; }

    // Navigation property
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    // Computed final outcome
    public InterviewOutcome FinalOutcome { get; set; } = InterviewOutcome.Pending;

    public DecisionMaker DecisionMaker { get; set; } = DecisionMaker.NotApplicable;

    public DateTime? OutcomeDate { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}