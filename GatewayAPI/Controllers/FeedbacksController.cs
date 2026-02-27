using GatewayAPI.Helpers;
using GatewayAPI.Services;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/feedbacks")]
[ExcludeFromCodeCoverage]
public class FeedbacksController : ControllerBase
{
    private readonly InterviewClient _client;

    public FeedbacksController(InterviewClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
        => await ProxyHelper.ProxyResponse(await _client.CreateFeedbackAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => await ProxyHelper.ProxyResponse(await _client.GetFeedbackByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFeedbackRequest request)
        => await ProxyHelper.ProxyResponse(await _client.UpdateFeedbackAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await ProxyHelper.ProxyResponse(await _client.DeleteFeedbackAsync(id));

    [HttpPatch("interview/{interviewId}/outcome")]
    public async Task<IActionResult> SetOutcome(int interviewId, [FromBody] SetOutcomeRequest request)
        => await ProxyHelper.ProxyResponse(await _client.SetOutcomeAsync(interviewId, request));

    [HttpGet("interview/{interviewId}/outcome")]
    public async Task<IActionResult> GetOutcome(int interviewId)
        => await ProxyHelper.ProxyResponse(await _client.GetOutcomeAsync(interviewId));
}