namespace ReportService.DTOs;

/// <summary>
/// Returned by GET /internal/api/reports/candidate.
/// Merges the old /summary, /candidate (paged stats), and a new paginated
/// all-candidates detailed section into a single response.
/// </summary>
public class CombinedCandidateReportResponse
{
    /// <summary>System-wide counts (candidates / interviews / requirements).</summary>
    public ReportSummaryResponse Summary { get; set; } = new();

    /// <summary>Paged aggregate stats: skills distribution, proficiency, availability.</summary>
    public CandidateReportPagedResponse CandidateStats { get; set; } = new();

    /// <summary>Paginated per-candidate detail (interviews + requirement matches).</summary>
    public AllCandidatesDetailedReportResponse AllCandidateDetails { get; set; } = new();
}

/// <summary>
/// Paginated list of detailed candidate reports —
/// same shape as CandidateDetailedReportResponse but wrapped in a page envelope.
/// </summary>
public class AllCandidatesDetailedReportResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public List<CandidateDetailedReportResponse> Candidates { get; set; } = new();
}