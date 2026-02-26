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

    // ── MERGED: summary + paged candidate stats + all-candidates detail ──────
    // GET /internal/api/reports/candidate
    //   ?page=1&pageSize=1000          → controls the candidate stats page
    //   &detailPage=1&detailPageSize=50 → controls the all-candidates detail page
    [HttpGet("candidate")]
    public async Task<IActionResult> GetCandidateReport(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000,
        [FromQuery] int detailPage = 1,
        [FromQuery] int detailPageSize = 50)
    {
        var result = await _reportManager.GetCombinedCandidateReportAsync(
            page, pageSize, detailPage, detailPageSize);

        return Ok(result);
    }

    // GET /internal/api/reports/candidate/{id}
    [HttpGet("candidate/{id:int}")]
    public async Task<IActionResult> GetCandidateDetailedReport(int id)
    {
        if (id < 0)
            return BadRequest("Id cannot be negative");

        var result = await _reportManager.GetCandidateDetailedReportAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // GET /internal/api/reports/interview-validation
    [HttpGet("interview-validation")]
    public async Task<IActionResult> GetInterviewValidationReport()
    {
        var result = await _reportManager.GetInterviewValidationReportAsync();
        return Ok(result);
    }

    // GET /internal/api/reports/requirement-fulfillment
    [HttpGet("requirement-fulfillment")]
    public async Task<IActionResult> GetRequirementFulfillmentReport(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reportManager.GetRequirementFulfillmentPagedAsync(page, pageSize);
        return Ok(result);
    }

    // GET /internal/api/reports/outcomes
    [HttpGet("outcomes")]
    public async Task<IActionResult> GetOutcomeReport()
    {
        var result = await _reportManager.GetOutcomeReportAsync();
        return Ok(result);
    }

    // GET /internal/api/reports/performance
    [HttpGet("performance")]
    public async Task<IActionResult> RunPerformanceTest([FromQuery] int requestCount = 20)
    {
        var result = await _reportManager.RunPerformanceTestAsync(requestCount);
        return Ok(result);
    }
}