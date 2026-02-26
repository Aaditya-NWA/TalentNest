using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class RequirementFulfillmentReportResponse
{
    public List<RequirementFulfillmentInfo> Requirements { get; set; } = new();
}
[ExcludeFromCodeCoverage]

public class RequirementFulfillmentInfo
{
    public int RequirementId { get; set; }

    public string Project { get; set; } = string.Empty;

    public int MatchedCandidates { get; set; }

    public int InterviewedCandidates { get; set; }

    public int SelectedCandidates { get; set; }

    public double FulfillmentPercentage { get; set; }
}
