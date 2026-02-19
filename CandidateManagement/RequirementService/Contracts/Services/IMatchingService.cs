using RequirementService.DTOs.Responses;

namespace RequirementService.Contracts.Services;

public interface IMatchingService
{
    Task<List<CandidateMatchResponse>> MatchCandidatesAsync(int requirementId);
}
