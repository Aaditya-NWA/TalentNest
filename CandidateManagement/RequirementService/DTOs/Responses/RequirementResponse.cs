namespace RequirementService.DTOs.Responses;

public class RequirementResponse
{
    public int Id { get; set; }
    public string Project { get; set; } = string.Empty;
    public string SkillsNeeded { get; set; } = string.Empty;
    public int MinExperienceMonths { get; set; }
    public int MaxExperienceMonths { get; set; }
    public DateTime AvailabilityStart { get; set; }
    public DateTime AvailabilityEnd { get; set; }
    public bool ClientInterviewRequired { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
