using Microsoft.EntityFrameworkCore;
using RequirementService.Contracts.Clients;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.DTOs.Responses;

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
        var candidates = await _candidateClient.GetAllCandidatesAsync();

        var matched = new List<CandidateMatchResponse>();

        foreach (var candidate in candidates)
        {
            if (IsMatch(requirement, candidate))
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
        }

        return matched;
    }

    private bool IsMatch(Models.Requirement requirement, CandidateDto candidate)
    {
        // Skill check (ALL required skills must exist)
        var requiredSkills = requirement.SkillsNeeded
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLower());

        var candidateSkills = candidate.SkillSet
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLower())
            .ToList();

        bool skillsMatch = requiredSkills
            .All(req => candidateSkills.Contains(req));

        // Experience check (>= minimum)
        bool experienceMatch =
            candidate.ExperienceMonths >= requirement.MinExperienceMonths;

        // Availability check (<= requirement start date)
        bool availabilityMatch =
            candidate.AvailabilityDate <= requirement.AvailabilityStart;

        // Primary skill level check
        int candidateLevel = int.Parse(candidate.PrimarySkillLevel.Substring(1));
        int requiredLevel = int.Parse(requirement.RequiredPrimarySkillLevel.Substring(1));

        bool proficiencyMatch = candidateLevel >= requiredLevel;

        return skillsMatch &&
               experienceMatch &&
               availabilityMatch &&
               proficiencyMatch;
    }
}
