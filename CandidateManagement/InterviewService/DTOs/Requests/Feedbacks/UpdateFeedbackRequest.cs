using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Feedbacks;

[ExcludeFromCodeCoverage]
public class UpdateFeedbackRequest
{
    [MaxLength(2000)]
    [Required]
    public string? Comments { get; set; }

    public InterviewOutcome? RecommendedOutcome { get; set; }
}