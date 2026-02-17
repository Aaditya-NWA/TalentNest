using InterviewService.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs.Requests.Interviews;

[ExcludeFromCodeCoverage]
public class SetOutcomeRequest
{
    [Required]
    public InterviewOutcome Outcome { get; set; }

    [Required]
    public DecisionMaker DecisionMaker { get; set; }
}