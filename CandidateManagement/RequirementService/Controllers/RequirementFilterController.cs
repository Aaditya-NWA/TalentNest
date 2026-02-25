using Microsoft.AspNetCore.Mvc;
using RequirementService.Contracts.Clients;
using RequirementService.DTOs.External;

namespace RequirementService.Controllers;

[ApiController]
[Route("internal/api/requirements/filter")]
public class RequirementFilterController : ControllerBase
{
    private readonly ICandidateClient _candidateClient;

    public RequirementFilterController(ICandidateClient candidateClient)
    {
        _candidateClient = candidateClient;
    }

    // ===============================
    // 1️⃣ FILTER BY SKILL
    // ===============================
    [HttpGet("by-skill")]
    public async Task<IActionResult> GetBySkill(
        [FromQuery] string skill,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (string.IsNullOrWhiteSpace(skill))
            return BadRequest("Skill is required.");

        if (page <= 0)
            return BadRequest("Page must be greater than zero.");

        if (pageSize <= 0 || pageSize > 200)
            return BadRequest("PageSize must be between 1 and 200.");

        var result = await _candidateClient.SearchCandidatesAsync(
            0,
            int.MaxValue,
            skill,
            null,
            null,   
            null,
  
            page,
            pageSize);

        return Ok(result);
    }

    // ===============================
    // 2️⃣ FILTER BY EXPERIENCE RANGE
    // ===============================
    [HttpGet("by-experience")]
    public async Task<IActionResult> GetByExperience(
        [FromQuery] int minExp,
        [FromQuery] int maxExp,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (minExp < 0 || maxExp < 0)
            return BadRequest("Experience cannot be negative.");

        if (minExp > maxExp)
            return BadRequest("minExp cannot be greater than maxExp.");

        if (page <= 0)
            return BadRequest("Page must be greater than zero.");

        if (pageSize <= 0 || pageSize > 200)
            return BadRequest("PageSize must be between 1 and 200.");

        var result = await _candidateClient.SearchCandidatesAsync(
            minExp,
            maxExp,
            null,
            null,
            null,
            null,
           
            page,
            pageSize);

        return Ok(result);
    }

    // ===============================
    // 3️⃣ FILTER BY AVAILABILITY
    // ===============================
    [HttpGet("by-availability")]
    public async Task<IActionResult> GetByAvailability(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (start > end)
            return BadRequest("Start date cannot be greater than End date.");

        if (page <= 0)
            return BadRequest("Page must be greater than zero.");

        if (pageSize <= 0 || pageSize > 200)
            return BadRequest("PageSize must be between 1 and 200.");

        var result = await _candidateClient.SearchCandidatesAsync(
            0,
            int.MaxValue,
            null,
            start,
            end,
            null,
   
            page,
            pageSize);

        return Ok(result);
    }

    // ===============================
    // 4️⃣ FILTER BY PRIMARY SKILL LEVEL
    // ===============================
    [HttpGet("by-primary-skill-level")]
    public async Task<IActionResult> GetByPrimarySkillLevel(
        [FromQuery] string level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (string.IsNullOrWhiteSpace(level))
            return BadRequest("Primary skill level is required.");

        if (page <= 0)
            return BadRequest("Page must be greater than zero.");

        if (pageSize <= 0 || pageSize > 200)
            return BadRequest("PageSize must be between 1 and 200.");

        var result = await _candidateClient.SearchCandidatesAsync(
            0,
            int.MaxValue,
            null,
            null,
            null,
            level,

            page,
            pageSize);

        return Ok(result);
    }


}