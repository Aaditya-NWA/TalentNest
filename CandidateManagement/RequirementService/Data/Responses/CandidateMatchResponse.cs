using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class CandidateMatchResponse
{
    public int CandidateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SkillSet { get; set; } = string.Empty;
    public int ExperienceMonths { get; set; }
    public DateTime AvailabilityDate { get; set; }
    public string PrimarySkillLevel { get; set; } = string.Empty;
}
