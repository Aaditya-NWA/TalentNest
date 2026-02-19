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
            var response = await _http
                .GetFromJsonAsync<CandidateListResponse>("/api/candidates");

            return response?.Data ?? new List<CandidateDto>();
        }
        catch (HttpRequestException)
        {
            return new List<CandidateDto>();
        }
    }

}
