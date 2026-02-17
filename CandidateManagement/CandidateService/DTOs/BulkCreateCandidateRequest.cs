using System.Diagnostics.CodeAnalysis;

namespace CandidateService.DTOs
{
    [ExcludeFromCodeCoverage]
    public class BulkCreateCandidateRequest
    {
        public List<CreateCandidateRequest> Candidates { get; set; } = new();
    }
}
