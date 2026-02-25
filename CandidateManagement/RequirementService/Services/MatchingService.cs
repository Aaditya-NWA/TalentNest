using Microsoft.EntityFrameworkCore;
using RequirementService.Contracts.Clients;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.DTOs.Responses;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Services;

public class MatchingService : IMatchingService
{
    private readonly RequirementDbContext _context;
    private readonly ICandidateClient _candidateClient;

    public MatchingService(
        RequirementDbContext context,
        ICandidateClient candidateClient)
    {
        _context = context;
        _candidateClient = candidateClient;
    }

    public async Task<List<CandidateMatchResponse>> MatchCandidatesAsync(int requirementId)
    {
        if (requirementId < 0)
            throw new ArgumentException("Id cannot be negative");
        // 1️⃣ Get requirement
        var requirement = await _context.Requirements
            .FirstOrDefaultAsync(r => r.Id == requirementId);

        if (requirement == null)
            throw new Exception("Requirement not found");

        // 2️⃣ Fetch all candidates
        // For better performance, we could implement a more advanced search in the Candidate Service 
        // var candidates = await _candidateClient.GetAllCandidatesAsync();
        var response = await _candidateClient.SearchCandidatesAsync(
            requirement.MinExperienceMonths,
            requirement.MaxExperienceMonths,
            requirement.SkillsNeeded,
            null,
            requirement.AvailabilityEnd,
            requirement.RequiredPrimarySkillLevel,
            1,
            200);

        var candidates = response.Data;

        var matched = new List<CandidateMatchResponse>();

        foreach (var candidate in candidates)
        {
            matched.Add(new CandidateMatchResponse
            {
                CandidateId = candidate.Id,
                Name = candidate.Name,
                SkillSet = candidate.SkillSet,
                ExperienceMonths = candidate.ExperienceMonths,
                AvailabilityDate = candidate.AvailabilityDate,
                PrimarySkillLevel = candidate.PrimarySkillLevel
            });
        }

        return matched
    .Take(200)
    .ToList();
    }
    [ExcludeFromCodeCoverage]
    public async Task<List<RankedCandidateDto>> GetRankedMatchesAsync(int requirementId)
    {
        if (requirementId <= 0)
            throw new ArgumentException("Id cannot be negative");

        var requirement = await _context.Requirements
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requirementId);

        if (requirement == null)
            throw new KeyNotFoundException("Requirement not found");

        var requiredSkills = requirement.SkillsNeeded
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLower())
            .ToList();

        // 🔥 OPTIMIZED: use filtered search instead of loading all
        var response = await _candidateClient.SearchCandidatesAsync(
            requirement.MinExperienceMonths,
            requirement.MaxExperienceMonths,
            requirement.SkillsNeeded,
            requirement.AvailabilityStart,
            requirement.AvailabilityEnd,
            requirement.RequiredPrimarySkillLevel,
            1,
            1000 // fetch larger page for ranking
        );

        var candidates = response.Data;

        var ranked = new List<RankedCandidateDto>();

        foreach (var candidate in candidates)
        {
            var candidateSkills = candidate.SkillSet
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();

            var matchedSkills = requiredSkills
                .Count(rs => candidateSkills.Contains(rs));

            double skillScore =
                requiredSkills.Count == 0
                    ? 0
                    : ((double)matchedSkills / requiredSkills.Count) * 50;

            double experienceScore =
                requirement.MinExperienceMonths == 0
                    ? 25
                    : candidate.ExperienceMonths >= requirement.MinExperienceMonths
                        ? 25
                        : ((double)candidate.ExperienceMonths / requirement.MinExperienceMonths) * 25;

            double availabilityScore =
                candidate.AvailabilityDate <= requirement.AvailabilityStart
                    ? 15
                    : 0;

            int candidateLevel = ParseLevel(candidate.PrimarySkillLevel);
            int requiredLevel = ParseLevel(requirement.RequiredPrimarySkillLevel);

            double proficiencyScore =
                requiredLevel == 0
                    ? 10
                    : candidateLevel >= requiredLevel
                        ? 10
                        : ((double)candidateLevel / requiredLevel) * 10;

            double totalScore =
                skillScore +
                experienceScore +
                availabilityScore +
                proficiencyScore;

            ranked.Add(new RankedCandidateDto
            {
                CandidateId = candidate.Id,
                Score = Math.Round(totalScore, 2),
                MatchedSkills = matchedSkills,
                TotalRequiredSkills = requiredSkills.Count,
                ExperienceMonths = candidate.ExperienceMonths,
                PrimarySkillLevel = candidate.PrimarySkillLevel,
                AvailabilityDate = candidate.AvailabilityDate,
                SkillSet = candidate.SkillSet
            });
        }

        return ranked
            .OrderByDescending(r => r.Score)
            .ToList();
    }
    [ExcludeFromCodeCoverage]
    private int ParseLevel(string level)
    {
        if (string.IsNullOrWhiteSpace(level) || level.Length < 2)
            return 0;

        return int.TryParse(level.Substring(1), out int result)
            ? result
            : 0;
    }
}
