using RequirementService.DTOs.Responses;

public interface IMatchingService
{
    Task<List<CandidateMatchResponse>> MatchCandidatesAsync(int requirementId);
    Task<List<RankedCandidateDto>> GetRankedMatchesAsync(int requirementId);
}
