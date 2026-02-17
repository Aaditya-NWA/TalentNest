using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Interviews;

[ExcludeFromCodeCoverage]
public class UpdateInterviewRequest
{
    [MaxLength(100)]
    public string? Project { get; set; }

    [MaxLength(100)]
    public string? Account { get; set; }

    [MaxLength(150)]
    public string? Interviewer { get; set; }

    public DateTime? InterviewDate { get; set; }

    [Range(1, 2)]
    public int? Level { get; set; }
}