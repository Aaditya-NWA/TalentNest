namespace ReportService.DTOs;

public class CandidateDto
{
    public int Id { get; set; }

    public string SkillSet { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public DateTime AvailabilityDate { get; set; }

    public string PrimarySkillLevel { get; set; } = string.Empty;
}
