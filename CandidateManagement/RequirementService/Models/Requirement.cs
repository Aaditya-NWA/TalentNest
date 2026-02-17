// RequirementService/Models/Requirement.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Models;

[Table("Requirements")]
[ExcludeFromCodeCoverage]
public class Requirement
{
    [Key]
    public int Id { get; set; }

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

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}