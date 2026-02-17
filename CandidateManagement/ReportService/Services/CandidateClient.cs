using System.Net.Http.Json;
using ReportService.DTOs;

namespace ReportService.Services;

public class CandidateClient
{
    private readonly HttpClient _http;
    public CandidateClient(HttpClient http) => _http = http;

    public async Task<List<CandidateDto>> GetAllAsync()
    {
        try
        {
            return await _http
                .GetFromJsonAsync<List<CandidateDto>>("/api/candidates")
                ?? [];
        }
        catch (HttpRequestException ex)
        {
            // log if you want
            return [];
        }
    }
}
