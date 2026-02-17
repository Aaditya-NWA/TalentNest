using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Interviews;

[ExcludeFromCodeCoverage]
public class CreateInterviewRequest
{
    [Required]
    public int CandidateId { get; set; }

    [Required]
    public int RequirementId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Project { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Interviewer { get; set; } = string.Empty;

    [Required]
    public DateTime InterviewDate { get; set; }

    [Required]
    [Range(1, 2)]
    public int Level { get; set; }
}