using FluentValidation;
using FluentValidation.AspNetCore;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Controllers;

[ApiController]
[Route("internal/api/interviews")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly IValidator<CreateInterviewRequest> _createInterviewValidator;
    private readonly IValidator<UpdateInterviewRequest> _updateInterviewValidator;

    public InterviewsController(
        IInterviewService interviewService,
        IValidator<CreateInterviewRequest> createInterviewValidator,
        IValidator<UpdateInterviewRequest> updateInterviewValidator)
    {
        _interviewService = interviewService;
        _createInterviewValidator = createInterviewValidator;
        _updateInterviewValidator = updateInterviewValidator;
    }

    /// <summary>
    /// Create Interview with 6-month rule validation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InterviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateInterview([FromBody] CreateInterviewRequest request)
    {
        var validationResult = await _createInterviewValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var interview = await _interviewService.CreateInterviewAsync(request);
            return CreatedAtAction(nameof(GetInterviewById), new { id = interview.Id }, interview);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message, code = "error" });
        }
    }

    /// <summary>
    ///  Get Interview by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InterviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInterviewById(int id)
    {
        var interview = await _interviewService.GetInterviewByIdAsync(id);

        if (interview == null)
            return NotFound(new { error = $"Interview with ID {id} not found." });

        return Ok(interview);
    }

    /// <summary>
    /// Get all interviews
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InterviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllInterviews()
    {
        var interviews = await _interviewService.GetAllInterviewsAsync();
        return Ok(interviews);
    }

    /// <summary>
    ///  Update Interview
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InterviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInterview(int id, [FromBody] UpdateInterviewRequest request)
    {
        var validationResult = await _updateInterviewValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        var interview = await _interviewService.UpdateInterviewAsync(id, request);

        if (interview == null)
            return NotFound(new { error = $"Interview with ID {id} not found." });

        return Ok(interview);
    }

    /// <summary>
    ///  Delete Interview
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInterview(int id)
    {
        var deleted = await _interviewService.DeleteInterviewAsync(id);

        if (!deleted)
            return NotFound(new { error = $"Interview with ID {id} not found." });

        return NoContent();
    }
}