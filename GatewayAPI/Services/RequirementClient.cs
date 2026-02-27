using RequirementService.DTOs.Requests;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class RequirementClient
{
    private readonly HttpClient _http;

    public RequirementClient(HttpClient http)
    {
        _http = http;
    }

    // ── Requirements ──────────────────────────────────────────

    public Task<HttpResponseMessage> CreateAsync(CreateRequirementRequest request)
        => _http.PostAsJsonAsync("/api/requirements", request);

    public Task<HttpResponseMessage> GetAllAsync(int page = 1, int pageSize = 50)
        => _http.GetAsync($"/api/requirements?page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> GetByIdAsync(int id)
        => _http.GetAsync($"/api/requirements/{id}");

    public Task<HttpResponseMessage> UpdateAsync(int id, CreateRequirementRequest request)
        => _http.PutAsJsonAsync($"/api/requirements/{id}", request);

    public Task<HttpResponseMessage> DeleteAsync(int id)
        => _http.DeleteAsync($"/api/requirements/{id}");

    public Task<HttpResponseMessage> MatchAsync(int id)
        => _http.GetAsync($"/api/requirements/{id}/match");

    public Task<HttpResponseMessage> GetCountAsync()
        => _http.GetAsync("/api/requirements/count");

    // ── Filters ───────────────────────────────────────────────

    public Task<HttpResponseMessage> FilterBySkillAsync(string skill, int page, int pageSize)
        => _http.GetAsync(
            $"/internal/api/requirements/filter/by-skill?skill={Uri.EscapeDataString(skill)}&page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> FilterByExperienceAsync(int minExp, int maxExp, int page, int pageSize)
        => _http.GetAsync(
            $"/internal/api/requirements/filter/by-experience?minExp={minExp}&maxExp={maxExp}&page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> FilterByAvailabilityAsync(DateTime start, DateTime end, int page, int pageSize)
        => _http.GetAsync(
            $"/internal/api/requirements/filter/by-availability?start={start:O}&end={end:O}&page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> FilterBySkillLevelAsync(string level, int page, int pageSize)
        => _http.GetAsync(
            $"/internal/api/requirements/filter/by-primary-skill-level?level={Uri.EscapeDataString(level)}&page={page}&pageSize={pageSize}");

    // ── Performance ───────────────────────────────────────────

    public Task<HttpResponseMessage> P95Async(int requirementId)
        => _http.GetAsync($"/api/performance/p95/{requirementId}");
}