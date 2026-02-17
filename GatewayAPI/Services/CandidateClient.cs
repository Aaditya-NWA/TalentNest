using GatewayAPI.DTOs.Candidates;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class CandidateClient
{
    private readonly HttpClient _http;

    public CandidateClient(HttpClient http)
    {
        _http = http;
    }

    // CREATE (single)
    public Task<HttpResponseMessage> CreateAsync(CreateCandidateRequests request)
    {
        return _http.PostAsJsonAsync("/api/candidates", request);
    }

    // GET ALL (paginated passthrough)
    public Task<HttpResponseMessage> GetAllAsync(int page, int pageSize)
    {
        return _http.GetAsync($"/api/candidates?page={page}&pageSize={pageSize}");
    }

    // GET BY ID
    public Task<HttpResponseMessage> GetByIdAsync(int id)
    {
        return _http.GetAsync($"/api/candidates/{id}");
    }

    // UPDATE
    public Task<HttpResponseMessage> UpdateAsync(int id, CreateCandidateRequests request)
    {
        return _http.PutAsJsonAsync($"/api/candidates/{id}", request);
    }

    // DELETE
    public Task<HttpResponseMessage> DeleteAsync(int id)
    {
        return _http.DeleteAsync($"/api/candidates/{id}");
    }

    // 🔥 BULK UPLOAD (PASSTHROUGH)
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
