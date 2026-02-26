using ReportService.DTOs;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace ReportService.Services;

[ExcludeFromCodeCoverage]
public class InterviewClient : IInterviewClient
{
    private readonly HttpClient _http;
    private readonly ILogger<InterviewClient> _logger;

    public InterviewClient(HttpClient http,
        ILogger<InterviewClient> logger)
    {
        _http = http;
        _logger = logger;
    }
    public async Task<InterviewCountDto> GetCountsAsync()
    {
        return await _http.GetFromJsonAsync<InterviewCountDto>(
            "/internal/api/interviews/count"
        ) ?? new InterviewCountDto(0, 0);
    }
    public async Task<List<InterviewDto>> GetByCandidateAsync(int candidateId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<InterviewDto>>(
                $"/internal/api/interviews/candidate/{candidateId}"
            ) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to fetch interviews for candidate {CandidateId}",
                candidateId);

            return new();
        }
    }

    public async Task<List<InterviewDto>> GetAllAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<InterviewDto>>(
                "/internal/api/interviews"
            ) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to fetch all interviews");

            return new();
        }
    }
    
}