using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Models
{
    [ExcludeFromCodeCoverage]
    public class CandidateStaging
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? MailId { get; set; }
        public string? SkillSet { get; set; }
        public int ExperienceMonths { get; set; }
        public DateTime AvailabilityDate { get; set; }
        public string? PrimarySkillLevel { get; set; }
    }
}
