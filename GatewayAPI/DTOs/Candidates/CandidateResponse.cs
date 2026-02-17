using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.DTOs.Candidates;

[ExcludeFromCodeCoverage]
public class CandidateResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MailId { get; set; } = string.Empty;

    public string SkillSet { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public DateTime AvailabilityDate { get; set; }

    public string PrimarySkillLevel { get; set; } = string.Empty;
}
