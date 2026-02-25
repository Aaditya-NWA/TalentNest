namespace ReportService.DTOs;

public class CandidateDetailedReportResponse
{
    public CandidateDto Candidate { get; set; } = new();

    public List<InterviewDto> Interviews { get; set; } = new();

    public List<RequirementSummaryDto> Requirements { get; set; } = new();

    public bool BlockedBySixMonthRule { get; set; }
}

public class RequirementSummaryDto
{
    public int RequirementId { get; set; }

    public string Project { get; set; } = string.Empty;

    public bool Matched { get; set; }

    public bool Interviewed { get; set; }

    public bool Selected { get; set; }
}