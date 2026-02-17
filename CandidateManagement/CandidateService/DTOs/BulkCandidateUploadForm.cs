using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.DTOs
{
    [ExcludeFromCodeCoverage]
    public class BulkCandidateUploadForm
    {
        public IFormFile File { get; set; } = null!;
    }
}
