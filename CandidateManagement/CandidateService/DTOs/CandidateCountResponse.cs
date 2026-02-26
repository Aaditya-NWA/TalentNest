using System.Diagnostics.CodeAnalysis;

namespace CandidateService.DTOs;

[ExcludeFromCodeCoverage]
public class CandidateCountResponse
{
    public int Total { get; set; }
    public int Available { get; set; }
}