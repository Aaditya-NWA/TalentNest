namespace ReportService.DTOs;

public class RequirementDto
{
    public int Id { get; set; }

    public string Project { get; set; } = string.Empty;

    public string SkillsNeeded { get; set; } = string.Empty;

    public int MinExperienceMonths { get; set; }

    public int MaxExperienceMonths { get; set; }

    public string RequiredPrimarySkillLevel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
