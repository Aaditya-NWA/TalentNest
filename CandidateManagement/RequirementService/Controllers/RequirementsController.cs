using Microsoft.AspNetCore.Mvc;
using RequirementService.Contracts.Services;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Controllers;

[ExcludeFromCodeCoverage]
[ApiController]
[Route("internal/api/requirements")]
public class RequirementsController : ControllerBase
{
    private readonly IRequirementService _requirementService;

    public RequirementsController(IRequirementService requirementService)
    {
        _requirementService = requirementService;
    }

    /// <summary>
    ///  DEMO: Create a new requirement
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RequirementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequirement([FromBody] CreateRequirementRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var requirement = await _requirementService.CreateRequirementAsync(request);
        return CreatedAtAction(nameof(GetRequirementById), new { id = requirement.Id }, requirement);
    }

    /// <summary>
    ///  DEMO: Get requirement by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RequirementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequirementById(int id)
    {
        var requirement = await _requirementService.GetRequirementByIdAsync(id);

        if (requirement == null)
            return NotFound(new { error = $"Requirement with ID {id} not found." });

        return Ok(requirement);
    }

    /// <summary>
    ///  DEMO: Get all requirements
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RequirementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRequirements()
    {
        var requirements = await _requirementService.GetAllRequirementsAsync();
        return Ok(requirements);
    }
}