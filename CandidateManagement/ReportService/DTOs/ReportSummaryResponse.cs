namespace ReportService.DTOs;

public class ReportSummaryResponse
{
    public int TotalCandidates { get; set; }
    public int TotalInterviews { get; set; }
    public int ScheduledInterviews { get; set; }
    public int OpenRequirements { get; set; }
}
