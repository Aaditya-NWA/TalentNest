using GatewayAPI.DTOs.Reports;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class ReportClient
{
    private readonly HttpClient _http;

    public ReportClient(HttpClient http)
    {
        _http = http;
    }

    public Task<ReportSummaryResponse?> GetSummaryAsync()
    {
        return _http.GetFromJsonAsync<ReportSummaryResponse>(
            "/api/reports/summary"
        );
    }
}
