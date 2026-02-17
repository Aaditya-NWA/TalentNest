using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Models;

[ExcludeFromCodeCoverage]
public class Feedback
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int InterviewId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comments { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int TechnicalScore { get; set; }

    [Required]
    [Range(1, 5)]
    public int CommunicationScore { get; set; }

    [Required]
    public InterviewOutcome RecommendedOutcome { get; set; }

    [MaxLength(500)]
    public string? Strengths { get; set; }

    [MaxLength(500)]
    public string? Weaknesses { get; set; }

    // Navigation property
    [ForeignKey("InterviewId")]
    public virtual Interview? Interview { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}