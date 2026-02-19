using RequirementService.DTOs.External;

namespace RequirementService.Contracts.Clients;

public interface ICandidateClient
{
    Task<List<CandidateDto>> GetAllCandidatesAsync();
}
