using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Feedbacks;

[ExcludeFromCodeCoverage]
public class AddFeedbackRequest
{
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

    [Required]
    public string CreatedBy { get; set; } = string.Empty;
}