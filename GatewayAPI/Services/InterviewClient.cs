using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;

public class InterviewClient
{
    private readonly HttpClient _http;

    public InterviewClient(HttpClient http)
    {
        _http = http;
    }

    // Interviews

    public Task<HttpResponseMessage> CreateAsync(InterviewService.DTOs.Requests.Interviews.CreateInterviewRequest request)
        => _http.PostAsJsonAsync("/api/interviews", request);

    public Task<HttpResponseMessage> GetByIdAsync(int id)
        => _http.GetAsync($"/api/interviews/{id}");

    public Task<HttpResponseMessage> GetAllAsync()
        => _http.GetAsync("/api/interviews");

    public Task<HttpResponseMessage> UpdateAsync(int id, UpdateInterviewRequest request)
        => _http.PutAsJsonAsync($"/api/interviews/{id}", request);

    public Task<HttpResponseMessage> DeleteAsync(int id)
        => _http.DeleteAsync($"/api/interviews/{id}");

    // Feedbacks

    public Task<HttpResponseMessage> CreateFeedbackAsync(CreateFeedbackRequest request)
        => _http.PostAsJsonAsync("/api/feedbacks", request);

    public Task<HttpResponseMessage> GetFeedbackByIdAsync(int id)
        => _http.GetAsync($"/api/feedbacks/{id}");

    public Task<HttpResponseMessage> UpdateFeedbackAsync(int id, UpdateFeedbackRequest request)
        => _http.PutAsJsonAsync($"/api/feedbacks/{id}", request);

    public Task<HttpResponseMessage> DeleteFeedbackAsync(int id)
        => _http.DeleteAsync($"/api/feedbacks/{id}");

    public Task<HttpResponseMessage> SetOutcomeAsync(int interviewId, SetOutcomeRequest request)
        => _http.PatchAsJsonAsync($"/api/feedbacks/interview/{interviewId}/outcome", request);

    public Task<HttpResponseMessage> GetOutcomeAsync(int interviewId)
        => _http.GetAsync($"/api/feedbacks/interview/{interviewId}/outcome");
}
