using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using InterviewService.Models.Enums;

namespace InterviewService.Contracts.Services;

public interface IInterviewService
{
    // ============ CREATE ============
    Task<InterviewResponse> CreateInterviewAsync(CreateInterviewRequest request);

    // ============ READ ============
    Task<InterviewResponse?> GetInterviewByIdAsync(int id);
    Task<IEnumerable<InterviewResponse>> GetAllInterviewsAsync();

    // ============ UPDATE ============
    Task<InterviewResponse?> UpdateInterviewAsync(int id, UpdateInterviewRequest request);
    Task<InterviewResponse> SetInterviewOutcomeAsync(int id, InterviewOutcome outcome, DecisionMaker decisionMaker);

    // ============ DELETE ============
    Task<bool> DeleteInterviewAsync(int id);
}
