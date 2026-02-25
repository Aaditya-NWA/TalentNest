using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class RankedCandidateDto
{
    public int CandidateId { get; set; }
    public double Score { get; set; }
    public int MatchedSkills { get; set; }
    public int TotalRequiredSkills { get; set; }
    public int ExperienceMonths { get; set; }
    public string PrimarySkillLevel { get; set; }
    public DateTime AvailabilityDate { get; set; }
    public string SkillSet { get; set; }
}
