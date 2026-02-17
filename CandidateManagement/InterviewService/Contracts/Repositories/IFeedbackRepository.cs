using InterviewService.Models;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Contracts.Repositories;


public interface IFeedbackRepository
{
    [ExcludeFromCodeCoverage]
    // ============ CREATE ============
    Task<Feedback> CreateAsync(Feedback feedback);

    // ============ READ ============
    Task<Feedback?> GetByIdAsync(int id);
    Task<IEnumerable<Feedback>> GetByInterviewIdAsync(int interviewId);
    Task<IEnumerable<Feedback>> GetAllAsync();                                      // ✅ ADD THIS
    Task<IEnumerable<Feedback>> GetAllAsync(DateTime? fromDate, DateTime? toDate); // ✅ ADD THIS
    Task<IEnumerable<Feedback>> GetByCreatedByAsync(string createdBy);             // ✅ OPTIONAL - for creator filter

    // ============ UPDATE ============
    Task<Feedback?> UpdateAsync(int id, Feedback feedback);

    // ============ DELETE ============
    Task<bool> DeleteAsync(int id);

    // ============ UTILITY ============
    Task<bool> ExistsAsync(int id);
    Task<int> GetFeedbackCountByInterviewAsync(int interviewId);
    Task<double> GetAverageScoreByInterviewAsync(int interviewId);
}