using System.Net.Http.Json;
using ReportService.DTOs;

namespace ReportService.Services;

public class CandidateClient
{
    private readonly HttpClient _http;

    public CandidateClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CandidateDto>> GetAllAsync()
    {
        var allCandidates = new List<CandidateDto>();

        int page = 1;
        int pageSize = 500; // Efficient batch size
        int totalPages;

        do
        {
            var response = await _http.GetFromJsonAsync<CandidateListResponse>(
                $"/api/candidates?page={page}&pageSize={pageSize}"
            );

            if (response == null || response.Data == null)
                break;

            allCandidates.AddRange(response.Data);

            totalPages = response.TotalPages;
            page++;

        } while (page <= totalPages);

        return allCandidates;
    }
}