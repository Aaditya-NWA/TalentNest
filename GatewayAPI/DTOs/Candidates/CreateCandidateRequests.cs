using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.DTOs.Candidates;

[ExcludeFromCodeCoverage]
public class CreateCandidateRequests
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string MailId { get; set; } = string.Empty;

    [Required]
    public string SkillSet { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int ExperienceMonths { get; set; }

    [Required]
    public DateTime AvailabilityDate { get; set; }

    [Required]
    public string PrimarySkillLevel { get; set; } = string.Empty;
}
