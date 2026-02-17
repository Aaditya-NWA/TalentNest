using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.DTOs
{
    [ExcludeFromCodeCoverage]
    public class CreateCandidateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string MailId { get; set; } = string.Empty;

        [Required]
        public string SkillSet { get; set; } = string.Empty;

        public int ExperienceMonths { get; set; }

        public DateTime AvailabilityDate { get; set; }


        [Required]
        [RegularExpression(@"(?i)^p[0-5]$", ErrorMessage = "PrimarySkillLevel must be one of: P0, P1, P2, P3, P4, P5 (case-insensitive).")]
        public string PrimarySkillLevel { get; set; } = "P0";
    }
}
