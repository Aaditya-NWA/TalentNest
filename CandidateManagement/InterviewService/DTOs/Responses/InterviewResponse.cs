using InterviewService.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class InterviewResponse
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int RequirementId { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Interviewer { get; set; } = string.Empty;
    public DateTime InterviewDate { get; set; }
    public int Level { get; set; }
    public InterviewOutcome FinalOutcome { get; set; }
    public DecisionMaker DecisionMaker { get; set; }
    public List<FeedbackResponse> Feedbacks { get; set; } = new();
}