using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public record InterviewCountDto(int Total, int Scheduled);