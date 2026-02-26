using System.Diagnostics.CodeAnalysis;

namespace ReportService.DTOs;

[ExcludeFromCodeCoverage]
public class RequirementFulfillmentPagedResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRequirements { get; set; }
    public int TotalPages { get; set; }

    public List<RequirementFulfillmentInfo> Requirements { get; set; } = new();
}