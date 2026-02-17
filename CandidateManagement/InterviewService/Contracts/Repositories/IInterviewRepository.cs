using InterviewService.Models;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Contracts.Repositories;

public interface IInterviewRepository
{
    [ExcludeFromCodeCoverage]
    // CREATE
    Task<Interview> CreateAsync(Interview interview);

    // READ
    Task<Interview?> GetByIdAsync(int id);
    Task<IEnumerable<Interview>> GetByCandidateIdAsync(int candidateId);
    Task<IEnumerable<Interview>> GetByRequirementIdAsync(int requirementId); // ✅ ADDED
    Task<IEnumerable<Interview>> GetAllAsync();
    Task<IEnumerable<Interview>> GetAllAsync(DateTime? fromDate, DateTime? toDate);

    // UPDATE
    Task<Interview?> UpdateAsync(int id, Interview interview);

    // DELETE
    Task<bool> DeleteAsync(int id);

    // VALIDATION & UTILITY
    Task<bool> ExistsAsync(int id);
    Task<(bool IsValid, string ErrorMessage)> HasInterviewedInLastSixMonthsAsync(int candidateId, string project, DateTime interviewDate);
    Task<IEnumerable<Interview>> GetInterviewsWithFeedbacksAsync(int candidateId);
}
