using RequirementService.DTOs.External;

namespace RequirementService.Contracts.Clients;

public interface ICandidateClient
{
    Task<List<CandidateDto>> GetAllCandidatesAsync();

    Task<PaginatedCandidateResponse> SearchCandidatesAsync(
        int minExp,
        int maxExp,
        string? skill,
        DateTime? start,
        DateTime? end,
        string? primarySkillLevel,
        int page,
        int pageSize);
}