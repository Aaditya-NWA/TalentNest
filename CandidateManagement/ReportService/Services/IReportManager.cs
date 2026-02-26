using ReportService.DTOs;
using System.Diagnostics.CodeAnalysis;

public interface IReportManager
{
    // ── Merged candidate endpoint (summary + paged stats + all-candidates detail) ──
    Task<CombinedCandidateReportResponse> GetCombinedCandidateReportAsync(
        int page,
        int pageSize,
        int detailPage,
        int detailPageSize);

    // ── Legacy / kept for internal use ──────────────────────────────────────────
    [ExcludeFromCodeCoverage]
    Task<ReportSummaryResponse> GetSystemSummaryAsync();

    Task<CandidateReportResponse> GetCandidateReportAsync(); // legacy – kept for back-compat

    Task<CandidateReportPagedResponse> GetCandidateReportPagedAsync(
        int page,
        int pageSize);

    Task<AllCandidatesDetailedReportResponse> GetAllCandidatesDetailedReportAsync(
        int page,
        int pageSize);

    // ── Other reports ────────────────────────────────────────────────────────────
    Task<InterviewValidationReportResponse> GetInterviewValidationReportAsync();
    Task<RequirementFulfillmentReportResponse> GetRequirementFulfillmentReportAsync();
    Task<OutcomeReportResponse> GetOutcomeReportAsync();
    Task<PerformanceReportResponse> RunPerformanceTestAsync(int requestCount);
    Task<CandidateDetailedReportResponse?> GetCandidateDetailedReportAsync(int candidateId);
    Task<RequirementFulfillmentPagedResponse> GetRequirementFulfillmentPagedAsync(int page, int pageSize);
}