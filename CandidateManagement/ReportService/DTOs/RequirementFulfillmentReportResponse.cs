namespace ReportService.DTOs;

public class RequirementFulfillmentReportResponse
{
    public List<RequirementFulfillmentInfo> Requirements { get; set; } = new();
}

public class RequirementFulfillmentInfo
{
    public int RequirementId { get; set; }

    public string Project { get; set; } = string.Empty;

    public int MatchedCandidates { get; set; }

    public int InterviewedCandidates { get; set; }

    public int SelectedCandidates { get; set; }

    public double FulfillmentPercentage { get; set; }
}
