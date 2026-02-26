using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class OutcomeReportResponse
{
    public int TotalInterviews { get; set; }

    public Dictionary<string, int> OutcomeBreakdown { get; set; } = new();
}
