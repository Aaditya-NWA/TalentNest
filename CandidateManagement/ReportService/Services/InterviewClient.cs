using System.Net.Http.Json;
using ReportService.DTOs;

namespace ReportService.Services;

public class InterviewClient : IInterviewClient
{
    private readonly HttpClient _http;
    private readonly ILogger<InterviewClient> _logger;

    public InterviewClient(HttpClient http, ILogger<InterviewClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<InterviewDto>> GetByCandidateAsync(int candidateId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<InterviewDto>>(
                $"/api/interviews/candidate/{candidateId}"
            ) ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Failed to fetch interviews for candidate {CandidateId}",
                candidateId);
            return [];
        }
    }
}
