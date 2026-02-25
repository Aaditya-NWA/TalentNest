using ReportService.DTOs;

public interface IReportManager
{
    Task<ReportSummaryResponse> GetSystemSummaryAsync();
    Task<CandidateReportResponse> GetCandidateReportAsync();
    Task<InterviewValidationReportResponse> GetInterviewValidationReportAsync();
    Task<RequirementFulfillmentReportResponse> GetRequirementFulfillmentReportAsync();
    Task<OutcomeReportResponse> GetOutcomeReportAsync();
    Task<PerformanceReportResponse> RunPerformanceTestAsync(int requestCount);

    // NEW
    Task<CandidateDetailedReportResponse?> GetCandidateDetailedReportAsync(int candidateId);
    
}