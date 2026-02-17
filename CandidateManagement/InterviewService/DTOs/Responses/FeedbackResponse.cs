using InterviewService.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class FeedbackResponse
{
    public int Id { get; set; }
    public int InterviewId { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string RecommendedOutcomeName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}