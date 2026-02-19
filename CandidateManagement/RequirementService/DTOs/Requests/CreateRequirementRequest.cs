using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class CreateRequirementRequest
{
    [Required]
    public string Project { get; set; } = string.Empty;

    [Required]
    public string SkillsNeeded { get; set; } = string.Empty;

    [Required]
    public string ExperienceRange { get; set; } = string.Empty;

    [Required]
    public string AvailabilityWindow { get; set; } = string.Empty;

    public bool ClientInterviewRequired { get; set; }
    [Required]
    [RegularExpression("P[0-5]")]
    public string RequiredPrimarySkillLevel { get; set; } = "P0";

}
