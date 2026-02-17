using System.Net.Http.Json;
using ReportService.DTOs;

namespace ReportService.Services;

public class RequirementClient
{
    private readonly HttpClient _http;
    public RequirementClient(HttpClient http) => _http = http;

    public async Task<List<RequirementDto>> GetAllAsync()
    {
        try
        {
            return await _http
                .GetFromJsonAsync<List<RequirementDto>>("/api/requirements")
                ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }
}
