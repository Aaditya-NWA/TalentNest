using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class PerformanceReportResponse
{
    public int TotalRequests { get; set; }

    public double AverageLatencyMs { get; set; }

    public double P95LatencyMs { get; set; }

    public double MinLatencyMs { get; set; }

    public double MaxLatencyMs { get; set; }
}
