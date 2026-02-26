using ReportService.DTOs;
using System.Net.Http.Json;

namespace ReportService.Services;

public class CandidateClient
{
    private readonly HttpClient _http;

    public CandidateClient(HttpClient http)
    {
        _http = http;
    }

    private const int BatchSize = 1000;

    public async Task<List<CandidateDto>> GetAllAsync()
    {
        var results = new List<CandidateDto>();

        int page = 1;

        while (true)
        {
            var response =
                await _http.GetFromJsonAsync<CandidateListResponse>(
                    $"/api/candidates?page={page}&pageSize={BatchSize}");

            if (response?.Data == null || response.Data.Count == 0)
                break;

            results.AddRange(response.Data);

            if (page >= response.TotalPages)
                break;

            page++;
        }

        return results;
    }

    public async Task<CandidateDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<CandidateDto>(
            $"/api/candidates/{id}");
    }


    public async Task<int> GetTotalCountAsync()
    {
        var response =
            await _http.GetFromJsonAsync<CandidateListResponse>(
                "/api/candidates?page=1&pageSize=1");

        return response?.TotalCount ?? 0;
    }
    public async Task<CandidateCountDto> GetCountsAsync()
    {
        return await _http.GetFromJsonAsync<CandidateCountDto>(
            "/api/candidates/count"
        ) ?? new(0, 0);
    }

    public async Task<CandidateListResponse> GetPageAsync(
    int page,
    int pageSize)
    {
        return await _http.GetFromJsonAsync<CandidateListResponse>(
            $"/api/candidates?page={page}&pageSize={pageSize}"
        ) ?? new CandidateListResponse();
    }
}