using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public record InterviewDto(
    int Id,
    int CandidateId,
    string Project,
    DateTime InterviewDate,
    int Level,
    string FinalOutcome,
    string DecisionMaker,
    List<FeedbackDto> Feedbacks
);

[ExcludeFromCodeCoverage]

public record FeedbackDto(
    int Id,
    int InterviewId,
    string Comments,
    string RecommendedOutcomeName,
    string CreatedBy
);