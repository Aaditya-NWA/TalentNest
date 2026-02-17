using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Feedbacks;

[ExcludeFromCodeCoverage]
public class CreateFeedbackRequest
{
    [Required]
    public int InterviewId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comments { get; set; } = string.Empty;

    [Required]
    public InterviewOutcome RecommendedOutcome { get; set; } = InterviewOutcome.Pending;

    [Required]
    public string CreatedBy { get; set; } = string.Empty;
}