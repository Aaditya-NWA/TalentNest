using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using InterviewService.Models;
using InterviewService.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IInterviewValidationService _validationService;

    public InterviewService(
        IInterviewRepository interviewRepository,
        IInterviewValidationService validationService)
    {
        _interviewRepository = interviewRepository;
        _validationService = validationService;
    }

    // ============ CREATE ============
    public async Task<InterviewResponse> CreateInterviewAsync(CreateInterviewRequest request)
    {
        var interview = new Interview
        {
            CandidateId = request.CandidateId,
            RequirementId = request.RequirementId,
            Project = request.Project,
            Account = request.Account,
            Interviewer = request.Interviewer,
            InterviewDate = request.InterviewDate,
            Level = (InterviewLevel)request.Level
        };

        // Validate before saving
        var validation = await _validationService.ValidateInterviewAsync(interview);
        if (!validation.IsValid)
            throw new InvalidOperationException(validation.ErrorMessage);

        var created = await _interviewRepository.CreateAsync(interview);
        return MapToResponse(created);
    }

    // ============ READ ============
    public async Task<InterviewResponse?> GetInterviewByIdAsync(int id)
    {
        var interview = await _interviewRepository.GetByIdAsync(id);
        return interview == null ? null : MapToResponse(interview);
    }

    public async Task<IEnumerable<InterviewResponse>> GetAllInterviewsAsync()
    {
        var interviews = await _interviewRepository.GetAllAsync();
        return interviews.Select(MapToResponse);
    }

    // ============ UPDATE ============
    public async Task<InterviewResponse?> UpdateInterviewAsync(int id, UpdateInterviewRequest request)
    {
        var existing = await _interviewRepository.GetByIdAsync(id);
        if (existing == null)
            return null;

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Project))
            existing.Project = request.Project;

        if (!string.IsNullOrEmpty(request.Account))
            existing.Account = request.Account;

        if (!string.IsNullOrEmpty(request.Interviewer))
            existing.Interviewer = request.Interviewer;

        if (request.InterviewDate.HasValue)
            existing.InterviewDate = request.InterviewDate.Value;

        if (request.Level.HasValue)
            existing.Level = (InterviewLevel)request.Level.Value;

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _interviewRepository.UpdateAsync(id, existing);
        return updated == null ? null : MapToResponse(updated);
    }

    public async Task<InterviewResponse> SetInterviewOutcomeAsync(int id, InterviewOutcome outcome, DecisionMaker decisionMaker)
    {
        var interview = await _interviewRepository.GetByIdAsync(id);
        if (interview == null)
            throw new KeyNotFoundException($"Interview with ID {id} not found.");

        interview.FinalOutcome = outcome;
        interview.DecisionMaker = decisionMaker;
        interview.OutcomeDate = DateTime.UtcNow;
        interview.UpdatedAt = DateTime.UtcNow;

        var updated = await _interviewRepository.UpdateAsync(id, interview);
        return MapToOutcomeResponse(updated!);
    }

    // ============ DELETE ============
    public async Task<bool> DeleteInterviewAsync(int id)
    {
        return await _interviewRepository.DeleteAsync(id);
    }

    // ============ PRIVATE METHODS ============
    private static InterviewResponse MapToResponse(Interview interview)
    {
        var feedbacks = interview.Feedbacks?.Select(f => new FeedbackResponse
        {
            Id = f.Id,
            InterviewId = f.InterviewId,
            Comments = f.Comments,
            RecommendedOutcomeName = f.RecommendedOutcome.ToString(),
            CreatedBy = f.CreatedBy
        }).ToList() ?? new();

        return new InterviewResponse
        {
            Id = interview.Id,
            CandidateId = interview.CandidateId,
            RequirementId = interview.RequirementId,
            Project = interview.Project,
            Account = interview.Account,
            InterviewDate = interview.InterviewDate,
            Level = (int)interview.Level,
            FinalOutcome = interview.FinalOutcome,
            //FinalOutcomeName = interview.FinalOutcome.ToString(),
            DecisionMaker = interview.DecisionMaker
        };
    }

    private static InterviewResponse MapToOutcomeResponse(Interview interview)
    {
        var feedbacks = interview.Feedbacks?.Select(f => new FeedbackResponse
        {
            Id = f.Id,
            InterviewId = f.InterviewId,
            Comments = f.Comments,
            RecommendedOutcomeName = f.RecommendedOutcome.ToString(),
            CreatedBy = f.CreatedBy
        }).ToList() ?? new();


        return new InterviewResponse
        {
            Id = interview.Id,
            CandidateId = interview.CandidateId,
            RequirementId = interview.RequirementId,
            Project = interview.Project,
            Account = interview.Account,
            Interviewer = interview.Interviewer,
            Level = (int)interview.Level,
            FinalOutcome = interview.FinalOutcome,
            DecisionMaker = interview.DecisionMaker
            //Feedbacks = feedbacks
        };
    }
}