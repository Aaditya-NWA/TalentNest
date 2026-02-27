using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.DTOs.Candidates;

[ExcludeFromCodeCoverage]
public class BulkCandidateForm
{
    public IFormFile? File { get; set; }
}