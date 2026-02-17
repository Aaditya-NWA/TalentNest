using System.ComponentModel.DataAnnotations;

namespace RequirementService.Models
{
    public class Requirement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Project { get; set; } = string.Empty;

        [Required]
        public string SkillsNeeded { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int MinExperienceMonths { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxExperienceMonths { get; set; }

        public DateTime AvailabilityStart { get; set; }
        public DateTime AvailabilityEnd { get; set; }

        public bool ClientInterviewRequired { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
