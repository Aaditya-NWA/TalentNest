using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Models
{
    [ExcludeFromCodeCoverage]
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string MailId { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string SkillSet { get; set; } = string.Empty;

        [Range(0, 600)]
        public int ExperienceMonths { get; set; }

        public DateTime AvailabilityDate { get; set; }

        [Required]
        [RegularExpression("P[0-5]")]
        public string PrimarySkillLevel { get; set; } = "P0";
    }
}

