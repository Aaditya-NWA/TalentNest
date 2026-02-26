using System.Diagnostics.CodeAnalysis;

namespace InterviewService.DTOs;

[ExcludeFromCodeCoverage]
public class InterviewCountResponse
{
    public int Total { get; set; }
    public int Scheduled { get; set; }
}