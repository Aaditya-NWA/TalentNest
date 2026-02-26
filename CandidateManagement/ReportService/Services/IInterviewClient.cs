using ReportService.DTOs;
using System.Diagnostics.CodeAnalysis;

public interface IInterviewClient
{
    [ExcludeFromCodeCoverage]
    Task<List<InterviewDto>> GetByCandidateAsync(int candidateId);
    Task<List<InterviewDto>> GetAllAsync();
    Task<InterviewCountDto> GetCountsAsync();
}
