using ReportService.DTOs;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace ReportService.Services;

[ExcludeFromCodeCoverage]
public class RequirementClient
{
    private readonly HttpClient _http;

    public RequirementClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RequirementDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("/api/requirements");
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<RequirementDto>>() ?? new();
    }

    public async Task MatchAsync(int id)
    {
        await _http.GetAsync($"/api/performance/p95/{id}");
    }
    public async Task<RequirementCountDto> GetCountsAsync()
    {
        return await _http.GetFromJsonAsync<RequirementCountDto>(
            "/api/requirements/count"
        ) ?? new RequirementCountDto(0);
    }
}