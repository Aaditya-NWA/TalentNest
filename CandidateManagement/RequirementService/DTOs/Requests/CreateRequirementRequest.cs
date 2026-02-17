using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class CreateRequirementRequest
{
    [Required]
    [MaxLength(100)]
    public string Project { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string SkillsNeeded { get; set; } = string.Empty;

    [Required]
    [Range(0, 600)]
    public int ExperienceMonths { get; set; }

    [Required]
    public DateTime AvailabilityWindow { get; set; }

    [Required]
    public bool ClientInterviewRequired { get; set; }
}