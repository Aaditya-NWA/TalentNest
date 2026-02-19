using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs.External;

[ExcludeFromCodeCoverage]
public class PaginatedCandidateResponse
{
    public List<CandidateDto> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
