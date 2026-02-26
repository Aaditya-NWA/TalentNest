using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class PagedRequirementResponse
{
    public List<RequirementDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}