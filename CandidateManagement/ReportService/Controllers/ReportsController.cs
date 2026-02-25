using Microsoft.AspNetCore.Mvc;
using ReportService.DTOs;
using ReportService.Services;

namespace ReportService.Controllers;

[ApiController]
[Route("internal/api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportManager _reportManager;


    public ReportsController(IReportManager reportManager)
    {
        _reportManager = reportManager;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _reportManager.GetSystemSummaryAsync();
        return Ok(result);
    }
    [HttpGet("candidate")]
    public async Task<IActionResult> GetCandidateReport()
    {
        var result = await _reportManager.GetCandidateReportAsync();
        return Ok(result);
    }
    [HttpGet("interview-validation")]
    public async Task<IActionResult> GetInterviewValidationReport()
    {
        var result = await _reportManager.GetInterviewValidationReportAsync();
        return Ok(result);
    }
    [HttpGet("requirement-fulfillment")]
    public async Task<IActionResult> GetRequirementFulfillmentReport()
    {
        var result = await _reportManager.GetRequirementFulfillmentReportAsync();
        return Ok(result);
    }
    [HttpGet("outcomes")]
    public async Task<IActionResult> GetOutcomeReport()
    {
        var result = await _reportManager.GetOutcomeReportAsync();
        return Ok(result);
    }
    [HttpGet("performance")]
    public async Task<IActionResult> RunPerformanceTest([FromQuery] int requestCount = 20)
    {
        var result = await _reportManager.RunPerformanceTestAsync(requestCount);
        return Ok(result);
    }
    [HttpGet("candidate/{id:int}")]
    public async Task<IActionResult> GetCandidateDetailedReport(int id)
    {
        if (id < 0)
            return BadRequest("Id cannot be negative");

        var result = await _reportManager
            .GetCandidateDetailedReportAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }


}
