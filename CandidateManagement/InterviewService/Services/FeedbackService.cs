using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Responses;
using InterviewService.Models;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IInterviewRepository _interviewRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IInterviewRepository interviewRepository)
    {
        _feedbackRepository = feedbackRepository;
        _interviewRepository = interviewRepository;
    }

    // ============ CREATE ============
    public async Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        var interviewExists = await _interviewRepository.ExistsAsync(request.InterviewId);
        if (!interviewExists)
            throw new KeyNotFoundException($"Interview with ID {request.InterviewId} not found.");

        var feedback = new Feedback
        {
            InterviewId = request.InterviewId,
            Comments = request.Comments,
            RecommendedOutcome = request.RecommendedOutcome,
            CreatedBy = request.CreatedBy
        };

        var created = await _feedbackRepository.CreateAsync(feedback);
        return MapToResponse(created);
    }

    // ============ READ ============
    public async Task<FeedbackResponse?> GetFeedbackByIdAsync(int id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        return feedback == null ? null : MapToResponse(feedback);
    }
    [ExcludeFromCodeCoverage]
    public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByInterviewAsync(int interviewId)
    {
        var feedbacks = await _feedbackRepository.GetByInterviewIdAsync(interviewId);
        return feedbacks.Select(MapToResponse);
    }

    // ✅ IMPLEMENTATION: Get all feedbacks
    [ExcludeFromCodeCoverage]
    public async Task<IEnumerable<FeedbackResponse>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();
        return feedbacks.Select(MapToResponse);
    }

    // ✅ IMPLEMENTATION: Get all feedbacks with date filters
    public async Task<IEnumerable<FeedbackResponse>> GetAllFeedbacksAsync(DateTime? fromDate, DateTime? toDate)
    {
        var feedbacks = await _feedbackRepository.GetAllAsync(fromDate, toDate);
        return feedbacks.Select(MapToResponse);
    }
    [ExcludeFromCodeCoverage]
    // ✅ IMPLEMENTATION: Get feedbacks by creator
    public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByCreatorAsync(string createdBy)
    {
        var feedbacks = await _feedbackRepository.GetByCreatedByAsync(createdBy);
        return feedbacks.Select(MapToResponse);
    }

    // ============ UPDATE ============
   
    public async Task<FeedbackResponse?> UpdateFeedbackAsync(int id, UpdateFeedbackRequest request)
    {
        var existing = await _feedbackRepository.GetByIdAsync(id);
        if (existing == null)
            return null;

        if (!string.IsNullOrEmpty(request.Comments))
            existing.Comments = request.Comments;

        if (request.RecommendedOutcome.HasValue)
            existing.RecommendedOutcome = request.RecommendedOutcome.Value;

        var updated = await _feedbackRepository.UpdateAsync(id, existing);
        return updated == null ? null : MapToResponse(updated);
    }

    // ============ DELETE ============
    public async Task<bool> DeleteFeedbackAsync(int id)
    {
        return await _feedbackRepository.DeleteAsync(id);
    }

    // ============ STATISTICS ============
    public async Task<double> GetAverageScoreForInterviewAsync(int interviewId)
    {
        return await _feedbackRepository.GetAverageScoreByInterviewAsync(interviewId);
    }

    public async Task<Dictionary<int, double>> GetAverageScoresForInterviewsAsync(IEnumerable<int> interviewIds)
    {
        var result = new Dictionary<int, double>();

        foreach (var id in interviewIds)
        {
            var avgScore = await _feedbackRepository.GetAverageScoreByInterviewAsync(id);
            result.Add(id, avgScore);
        }

        return result;
    }

    // ============ PRIVATE METHODS ============
    private static FeedbackResponse MapToResponse(Feedback feedback)
    {
        return new FeedbackResponse
        {
            Id = feedback.Id,
            InterviewId = feedback.InterviewId,
            Comments = feedback.Comments,
            RecommendedOutcomeName = feedback.RecommendedOutcome.ToString(),
            CreatedBy = feedback.CreatedBy
        };
    }
}