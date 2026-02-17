using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class RequirementResponse
{
    public int Id { get; set; }
    public string Project { get; set; } = string.Empty;
    public string SkillsNeeded { get; set; } = string.Empty;
    public int ExperienceMonths { get; set; }
    public DateTime AvailabilityWindow { get; set; }
    public bool ClientInterviewRequired { get; set; }
    public DateTime CreatedAt { get; set; }
}