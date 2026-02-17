using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.DTOs.Requirements;

[ExcludeFromCodeCoverage]
public class CreateRequirementRequests
{
    public string Title { get; set; } = string.Empty;
    public string SkillSet { get; set; } = string.Empty;
    public int ExperienceMonths { get; set; }
    public int OpenPositions { get; set; }
}
