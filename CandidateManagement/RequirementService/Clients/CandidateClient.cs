using RequirementService.Contracts.Clients;
using RequirementService.DTOs.External;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace RequirementService.Clients;

[ExcludeFromCodeCoverage]
public class CandidateClient : ICandidateClient
{
    private readonly HttpClient _httpClient;

    public CandidateClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CandidateDto>> GetAllCandidatesAsync()
    {
        var allCandidates = new List<CandidateDto>();

        int currentPage = 1;
        int totalPages = 1;
        int pageSize = 200; // maximum allowed

        do
        {
            var response = await _httpClient
                .GetFromJsonAsync<PaginatedCandidateResponse>(
                    $"/api/candidates?page={currentPage}&pageSize={pageSize}");

            if (response == null)
                break;

            allCandidates.AddRange(response.Data);

            totalPages = response.TotalPages;
            currentPage++;

        } while (currentPage <= totalPages);

        return allCandidates;
    }
}
