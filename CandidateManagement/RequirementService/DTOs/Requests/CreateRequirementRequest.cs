using System.ComponentModel.DataAnnotations;

namespace RequirementService.DTOs.Requests;

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
}
