public class CandidateReportPagedResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalPages { get; set; }

    public Dictionary<string, int> SkillsDistribution { get; set; } = new();
    public Dictionary<string, int> ProficiencyDistribution { get; set; } = new();

    public int AvailableCandidates { get; set; }
    public double AvailabilityPercentage { get; set; }

    public List<int> BlockedCandidatesLast6Months { get; set; } = new();
}