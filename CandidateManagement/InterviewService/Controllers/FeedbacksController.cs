using FluentValidation;
using FluentValidation.AspNetCore;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Controllers;

[ApiController]
[Route("internal/api/feedbacks")]
public class FeedbacksController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly IInterviewService _interviewService;
    private readonly IValidator<CreateFeedbackRequest> _createFeedbackValidator;
    private readonly IValidator<UpdateFeedbackRequest> _updateFeedbackValidator;
    private readonly IValidator<SetOutcomeRequest> _setOutcomeValidator;

    public FeedbacksController(
        IFeedbackService feedbackService,
        IInterviewService interviewService,
        IValidator<CreateFeedbackRequest> createFeedbackValidator,
        IValidator<UpdateFeedbackRequest> updateFeedbackValidator,
        IValidator<SetOutcomeRequest> setOutcomeValidator)
    {
        _feedbackService = feedbackService;
        _interviewService = interviewService;
        _createFeedbackValidator = createFeedbackValidator;
        _updateFeedbackValidator = updateFeedbackValidator;
        _setOutcomeValidator = setOutcomeValidator;
    }

    /// <summary>
    ///  Create Feedback
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        var validationResult = await _createFeedbackValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var feedback = await _feedbackService.CreateFeedbackAsync(request);
            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    ///  Get Feedback by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeedbackById(int id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

        if (feedback == null)
            return NotFound(new { error = $"Feedback with ID {id} not found." });

        return Ok(feedback);
    }

    /// <summary>
    ///  Update Feedback
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackRequest request)
    {
        var validationResult = await _updateFeedbackValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        var feedback = await _feedbackService.UpdateFeedbackAsync(id, request);

        if (feedback == null)
            return NotFound(new { error = $"Feedback with ID {id} not found." });

        return Ok(feedback);
    }

    /// <summary>
    ///  Delete Feedback
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        var deleted = await _feedbackService.DeleteFeedbackAsync(id);

        if (!deleted)
            return NotFound(new { error = $"Feedback with ID {id} not found." });

        return NoContent();
    }

    /// <summary>
    ///  Set Interview Outcome (Selected/Rejected + Decision Maker)
    /// This belongs in FeedbacksController because outcome is the final result of the feedback process
    /// </summary>
    [HttpPatch("interview/{interviewId}/outcome")]
    [ProducesResponseType(typeof(InterviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetInterviewOutcome(int interviewId, [FromBody] SetOutcomeRequest request)
    {
        var validationResult = await _setOutcomeValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var interview = await _interviewService.SetInterviewOutcomeAsync(interviewId, request.Outcome, request.DecisionMaker);
            return Ok(interview);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    ///  Get Interview Outcome
    /// </summary>
    [HttpGet("interview/{interviewId}/outcome")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInterviewOutcome(int interviewId)
    {
        var interview = await _interviewService.GetInterviewByIdAsync(interviewId);

        if (interview == null)
            return NotFound(new { error = $"Interview with ID {interviewId} not found." });

        return Ok(new
        {
            interviewId = interview.Id,
            candidateId = interview.CandidateId,
            project = interview.Project,
            finalOutcome = interview.FinalOutcome,
            //finalOutcomeName = interview.FinalOutcomeName,
            decisionMaker = interview.DecisionMaker,
            //decisionMakerName = interview.DecisionMakerName,
            //outcomeDate = interview.OutcomeDate
        });
    }
}