using ReportService.DTOs;

public interface IInterviewClient
{
    Task<List<InterviewDto>> GetByCandidateAsync(int candidateId);
    Task<List<InterviewDto>> GetAllAsync();
    Task<InterviewCountDto> GetCountsAsync();
}
