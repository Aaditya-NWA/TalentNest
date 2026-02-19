namespace ReportService.DTOs;

public record InterviewDto(
    int Id,
    int CandidateId,
    string Project,
    DateTime InterviewDate,
    string Status,
    string Outcome
);
