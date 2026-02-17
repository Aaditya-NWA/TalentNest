using InterviewService.Models;

namespace InterviewService.Contracts.Services;

public interface IInterviewValidationService
{
    Task<(bool IsValid, string ErrorMessage)> ValidateInterviewAsync(Interview interview);
    Task<bool> ValidateInterviewLevelAsync(int level, bool clientInterviewRequired);
    Task<(bool IsValid, string ErrorMessage)> HasInterviewedInLastSixMonthsAsync(int candidateId, string project, DateTime interviewDate);
}