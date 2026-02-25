using ReportService.DTOs;
using System.Net.Http.Json;

namespace ReportService.Services;

public class RequirementClient
{
    private readonly HttpClient _http;

    public RequirementClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RequirementDto>> GetAllAsync()
    {
        try
        {
            return await _http
                .GetFromJsonAsync<List<RequirementDto>>("/api/requirements")
                ?? new List<RequirementDto>();
        }
        catch
        {
            return new List<RequirementDto>();
        }
    }

    public async Task MatchAsync(int id)
    {
        await _http.GetAsync($"/api/requirements/{id}/match");
    }
}