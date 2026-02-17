using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Responses;

namespace InterviewService.Contracts.Services;

public interface IFeedbackService
{
    // ============ CREATE ============
    Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request);

    // ============ READ ============
    Task<FeedbackResponse?> GetFeedbackByIdAsync(int id);
    Task<IEnumerable<FeedbackResponse>> GetFeedbacksByInterviewAsync(int interviewId);
    Task<IEnumerable<FeedbackResponse>> GetAllFeedbacksAsync();                          // ✅ ADD THIS
    Task<IEnumerable<FeedbackResponse>> GetAllFeedbacksAsync(DateTime? fromDate, DateTime? toDate); // ✅ ADD THIS
    Task<IEnumerable<FeedbackResponse>> GetFeedbacksByCreatorAsync(string createdBy);    // ✅ ADD THIS

    // ============ UPDATE ============
    Task<FeedbackResponse?> UpdateFeedbackAsync(int id, UpdateFeedbackRequest request);

    // ============ DELETE ============
    Task<bool> DeleteFeedbackAsync(int id);

    // ============ STATISTICS ============
    Task<double> GetAverageScoreForInterviewAsync(int interviewId);                     // ✅ ADD THIS
    Task<Dictionary<int, double>> GetAverageScoresForInterviewsAsync(IEnumerable<int> interviewIds); // ✅ ADD THIS
}