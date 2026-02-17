using Microsoft.AspNetCore.Mvc;
using ReportService.DTOs;
using ReportService.Services;

namespace ReportService.Controllers;

[ApiController]
[Route("internal/api/reports")]
public class ReportsController : ControllerBase
{
    private readonly CandidateClient _candidates;
    private readonly IInterviewClient _interviews;
    private readonly RequirementClient _requirements;

    public ReportsController(
        CandidateClient candidates,
        IInterviewClient interviews,
        RequirementClient requirements)
    {
        _candidates = candidates;
        _interviews = interviews;
        _requirements = requirements;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var candidates = await _candidates.GetAllAsync();
        var requirements = await _requirements.GetAllAsync();

        var interviewTasks = candidates
            .Select(c => _interviews.GetByCandidateAsync(c.Id))
            .ToList();

        var interviewResults = await Task.WhenAll(interviewTasks);

        var allInterviews = interviewResults
            .SelectMany(i => i)
            .ToList();

        var report = new ReportSummaryResponse
        {
            TotalCandidates = candidates.Count,
            TotalInterviews = allInterviews.Count,
            ScheduledInterviews =
                allInterviews.Count(i => i.Status == "Scheduled"),
            OpenRequirements =
                requirements.Count(r => r.Status == "Open")
        };

        return Ok(report);
    }

}
