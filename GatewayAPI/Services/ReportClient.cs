using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class ReportClient
{
    private readonly HttpClient _http;

    public ReportClient(HttpClient http)
    {
        _http = http;
    }

    // GET /internal/api/reports/candidate
    public Task<HttpResponseMessage> GetCandidateReportAsync(
        int page = 1,
        int pageSize = 1000,
        int detailPage = 1,
        int detailPageSize = 50)
        => _http.GetAsync(
            $"/internal/api/reports/candidate?page={page}&pageSize={pageSize}&detailPage={detailPage}&detailPageSize={detailPageSize}");

    // GET /internal/api/reports/candidate/{id}
    public Task<HttpResponseMessage> GetCandidateDetailedAsync(int id)
        => _http.GetAsync($"/internal/api/reports/candidate/{id}");

    // GET /internal/api/reports/interview-validation
    public Task<HttpResponseMessage> GetInterviewValidationAsync()
        => _http.GetAsync("/internal/api/reports/interview-validation");

    // GET /internal/api/reports/requirement-fulfillment
    public Task<HttpResponseMessage> GetRequirementFulfillmentAsync(int page = 1, int pageSize = 20)
        => _http.GetAsync(
            $"/internal/api/reports/requirement-fulfillment?page={page}&pageSize={pageSize}");

    // GET /internal/api/reports/outcomes
    public Task<HttpResponseMessage> GetOutcomesAsync()
        => _http.GetAsync("/internal/api/reports/outcomes");

    // GET /internal/api/reports/performance
    public Task<HttpResponseMessage> RunPerformanceAsync(int requestCount = 20)
        => _http.GetAsync($"/internal/api/reports/performance?requestCount={requestCount}");
}