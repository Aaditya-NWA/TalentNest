using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.DTOs
{
    [ExcludeFromCodeCoverage]
    public class BulkCandidateForm
    {
        public IFormFile File { get; set; } = null!;
    }
}
