namespace ReportService.DTOs;

public class RequirementResponse
{
    public int Id { get; set; }
    public string Project { get; set; } = "";
    public string SkillsNeeded { get; set; } = "";
    public int MinExperienceMonths { get; set; }
    public int MaxExperienceMonths { get; set; }
    public DateTime AvailabilityStart { get; set; }
    public DateTime AvailabilityEnd { get; set; }
    public bool ClientInterviewRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RequiredPrimarySkillLevel { get; set; } = "";
}