using System.Diagnostics.CodeAnalysis;
using RequirementService.Models;

namespace RequirementService.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class PaginatedRequirementResponse
{
    public List<Requirement> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}