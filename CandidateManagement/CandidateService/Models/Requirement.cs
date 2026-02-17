using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Models
{
    [ExcludeFromCodeCoverage]
    public class Requirement
    {
        [Key]
        public int Id { get; set; }
    }
}
