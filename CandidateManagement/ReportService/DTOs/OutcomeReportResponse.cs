namespace ReportService.DTOs;

public class OutcomeReportResponse
{
    public int TotalInterviews { get; set; }

    public Dictionary<string, int> OutcomeBreakdown { get; set; } = new();
}
