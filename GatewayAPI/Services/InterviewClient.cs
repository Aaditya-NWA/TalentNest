using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Services;

[ExcludeFromCodeCoverage]
public class InterviewClient
{
    private readonly HttpClient _http;

    public InterviewClient(HttpClient http)
    {
        _http = http;
    }

    // ── Interviews ────────────────────────────────────────────

    public Task<HttpResponseMessage> CreateInterviewAsync(CreateInterviewRequest request)
        => _http.PostAsJsonAsync("/internal/api/interviews", request);

    public Task<HttpResponseMessage> GetAllInterviewsAsync()
        => _http.GetAsync("/internal/api/interviews");

    public Task<HttpResponseMessage> GetInterviewByIdAsync(int id)
        => _http.GetAsync($"/internal/api/interviews/{id}");

    public Task<HttpResponseMessage> UpdateInterviewAsync(int id, UpdateInterviewRequest request)
        => _http.PutAsJsonAsync($"/internal/api/interviews/{id}", request);

    public Task<HttpResponseMessage> DeleteInterviewAsync(int id)
        => _http.DeleteAsync($"/internal/api/interviews/{id}");

    public Task<HttpResponseMessage> GetInterviewsByCandidateAsync(int candidateId)
        => _http.GetAsync($"/internal/api/interviews/candidate/{candidateId}");

    public Task<HttpResponseMessage> GetInterviewCountAsync()
        => _http.GetAsync("/internal/api/interviews/count");

    // ── Feedbacks ─────────────────────────────────────────────

    public Task<HttpResponseMessage> CreateFeedbackAsync(CreateFeedbackRequest request)
        => _http.PostAsJsonAsync("/internal/api/feedbacks", request);

    public Task<HttpResponseMessage> GetFeedbackByIdAsync(int id)
        => _http.GetAsync($"/internal/api/feedbacks/{id}");

    public Task<HttpResponseMessage> UpdateFeedbackAsync(int id, UpdateFeedbackRequest request)
        => _http.PutAsJsonAsync($"/internal/api/feedbacks/{id}", request);

    public Task<HttpResponseMessage> DeleteFeedbackAsync(int id)
        => _http.DeleteAsync($"/internal/api/feedbacks/{id}");

    public Task<HttpResponseMessage> SetOutcomeAsync(int interviewId, SetOutcomeRequest request)
        => _http.PatchAsJsonAsync($"/internal/api/feedbacks/interview/{interviewId}/outcome", request);

    public Task<HttpResponseMessage> GetOutcomeAsync(int interviewId)
        => _http.GetAsync($"/internal/api/feedbacks/interview/{interviewId}/outcome");
}