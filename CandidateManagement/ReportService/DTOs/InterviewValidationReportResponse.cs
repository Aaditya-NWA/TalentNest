using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class InterviewValidationReportResponse
{
    public List<BlockedCandidateInfo> BlockedCandidates { get; set; } = new();
}

[ExcludeFromCodeCoverage]

public class BlockedCandidateInfo
{
    public int CandidateId { get; set; }

    public string Project { get; set; } = string.Empty;

    public DateTime LastInterviewDate { get; set; }

    public int DaysUntilEligible { get; set; }
}
