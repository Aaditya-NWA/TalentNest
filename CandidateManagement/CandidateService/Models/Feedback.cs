using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Models
{
    [ExcludeFromCodeCoverage]
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
    }
}

