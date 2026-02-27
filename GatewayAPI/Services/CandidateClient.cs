using GatewayAPI.DTOs.Candidates;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class CandidateClient
{
    private readonly HttpClient _http;

    public CandidateClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> CreateAsync(CreateCandidateRequests request)
        => _http.PostAsJsonAsync("/api/candidates", request);

    public Task<HttpResponseMessage> GetAllAsync(int page, int pageSize)
        => _http.GetAsync($"/api/candidates?page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> GetByIdAsync(int id)
        => _http.GetAsync($"/api/candidates/{id}");

    public Task<HttpResponseMessage> UpdateAsync(int id, CreateCandidateRequests request)
        => _http.PutAsJsonAsync($"/api/candidates/{id}", request);

    public Task<HttpResponseMessage> DeleteAsync(int id)
        => _http.DeleteAsync($"/api/candidates/{id}");

    public Task<HttpResponseMessage> SearchAsync(
        string? skill, int? minExp, int? maxExp,
        string? primarySkillLevel, int page, int pageSize,
        string? start = null, string? end = null)
    {
        var query = $"/api/candidates/search?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(skill)) query += $"&skill={Uri.EscapeDataString(skill)}";
        if (minExp.HasValue) query += $"&minExp={minExp}";
        if (maxExp.HasValue) query += $"&maxExp={maxExp}";
        if (!string.IsNullOrWhiteSpace(primarySkillLevel)) query += $"&primarySkillLevel={Uri.EscapeDataString(primarySkillLevel)}";
        if (!string.IsNullOrWhiteSpace(start)) query += $"&start={Uri.EscapeDataString(start)}";
        if (!string.IsNullOrWhiteSpace(end)) query += $"&end={Uri.EscapeDataString(end)}";
        return _http.GetAsync(query);
    }

    public Task<HttpResponseMessage> GetCountAsync()
        => _http.GetAsync("/api/candidates/count");

    public async Task<HttpResponseMessage> BulkUploadAsync(IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        content.Add(streamContent, "file", file.FileName);
        return await _http.PostAsync("/api/candidates/bulk", content);
    }
}