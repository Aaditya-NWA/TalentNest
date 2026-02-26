using ReportService.DTOs;

public interface IReportManager
{
    Task<ReportSummaryResponse> GetSystemSummaryAsync();
    Task<CandidateReportResponse> GetCandidateReportAsync(); // legacy
    Task<CandidateReportPagedResponse> GetCandidateReportPagedAsync(
        int page,
        int pageSize);

    Task<InterviewValidationReportResponse> GetInterviewValidationReportAsync();
    Task<RequirementFulfillmentReportResponse> GetRequirementFulfillmentReportAsync();
    Task<OutcomeReportResponse> GetOutcomeReportAsync();
    Task<PerformanceReportResponse> RunPerformanceTestAsync(int requestCount);
    Task<CandidateDetailedReportResponse?> GetCandidateDetailedReportAsync(int candidateId);
}