using GatewayAPI.Helpers;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/reports")]
[ExcludeFromCodeCoverage]
public class ReportsController : ControllerBase
{
    private readonly ReportClient _client;

    public ReportsController(ReportClient client)
    {
        _client = client;
    }

    // GET /api/reports/candidate
    [HttpGet("candidate")]
    public async Task<IActionResult> GetCandidateReport(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000,
        [FromQuery] int detailPage = 1,
        [FromQuery] int detailPageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.GetCandidateReportAsync(page, pageSize, detailPage, detailPageSize));

    // GET /api/reports/candidate/{id}
    [HttpGet("candidate/{id:int}")]
    public async Task<IActionResult> GetCandidateDetailed(int id)
        => await ProxyHelper.ProxyResponse(await _client.GetCandidateDetailedAsync(id));

    // GET /api/reports/interview-validation
    [HttpGet("interview-validation")]
    public async Task<IActionResult> GetInterviewValidation()
        => await ProxyHelper.ProxyResponse(await _client.GetInterviewValidationAsync());

    // GET /api/reports/requirement-fulfillment
    [HttpGet("requirement-fulfillment")]
    public async Task<IActionResult> GetRequirementFulfillment(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => await ProxyHelper.ProxyResponse(
            await _client.GetRequirementFulfillmentAsync(page, pageSize));

    // GET /api/reports/outcomes
    [HttpGet("outcomes")]
    public async Task<IActionResult> GetOutcomes()
        => await ProxyHelper.ProxyResponse(await _client.GetOutcomesAsync());

    // GET /api/reports/performance
    [HttpGet("performance")]
    public async Task<IActionResult> RunPerformance(
        [FromQuery] int requestCount = 20)
        => await ProxyHelper.ProxyResponse(
            await _client.RunPerformanceAsync(requestCount));
}