using GatewayAPI.Helpers;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RequirementService.DTOs.Requests;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

// ── Core Requirements ─────────────────────────────────────────────────────────

[ApiController]
[Route("api/requirements")]
[ExcludeFromCodeCoverage]
public class RequirementsController : ControllerBase
{
    private readonly RequirementClient _client;

    public RequirementsController(RequirementClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequirementRequest request)
        => await ProxyHelper.ProxyResponse(await _client.CreateAsync(request));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(await _client.GetAllAsync(page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => await ProxyHelper.ProxyResponse(await _client.GetByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRequirementRequest request)
        => await ProxyHelper.ProxyResponse(await _client.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await ProxyHelper.ProxyResponse(await _client.DeleteAsync(id));

    [HttpGet("{id}/match")]
    public async Task<IActionResult> Match(int id)
        => await ProxyHelper.ProxyResponse(await _client.MatchAsync(id));

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
        => await ProxyHelper.ProxyResponse(await _client.GetCountAsync());
}

// ── Requirement Filters ───────────────────────────────────────────────────────

[ApiController]
[Route("api/requirements/filter")]
[ExcludeFromCodeCoverage]
public class RequirementFilterController : ControllerBase
{
    private readonly RequirementClient _client;

    public RequirementFilterController(RequirementClient client)
    {
        _client = client;
    }

    [HttpGet("by-skill")]
    public async Task<IActionResult> BySkill(
        [FromQuery] string skill,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.FilterBySkillAsync(skill, page, pageSize));

    [HttpGet("by-experience")]
    public async Task<IActionResult> ByExperience(
        [FromQuery] int minExp,
        [FromQuery] int maxExp,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.FilterByExperienceAsync(minExp, maxExp, page, pageSize));

    [HttpGet("by-availability")]
    public async Task<IActionResult> ByAvailability(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.FilterByAvailabilityAsync(start, end, page, pageSize));

    [HttpGet("by-primary-skill-level")]
    public async Task<IActionResult> BySkillLevel(
        [FromQuery] string level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.FilterBySkillLevelAsync(level, page, pageSize));
}

// ── Performance ───────────────────────────────────────────────────────────────

[ApiController]
[Route("api/performance")]
[ExcludeFromCodeCoverage]
public class PerformanceController : ControllerBase
{
    private readonly RequirementClient _client;

    public PerformanceController(RequirementClient client)
    {
        _client = client;
    }

    [HttpGet("p95/{requirementId}")]
    public async Task<IActionResult> P95(int requirementId)
        => await ProxyHelper.ProxyResponse(await _client.P95Async(requirementId));
}