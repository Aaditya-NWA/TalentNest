using GatewayAPI.Helpers;
using GatewayAPI.Services;
using InterviewService.DTOs.Requests.Interviews;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/interviews")]
[ExcludeFromCodeCoverage]
public class InterviewsController : ControllerBase
{
    private readonly InterviewClient _client;

    public InterviewsController(InterviewClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInterviewRequest request)
        => await ProxyHelper.ProxyResponse(await _client.CreateInterviewAsync(request));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => await ProxyHelper.ProxyResponse(await _client.GetAllInterviewsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => await ProxyHelper.ProxyResponse(await _client.GetInterviewByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInterviewRequest request)
        => await ProxyHelper.ProxyResponse(await _client.UpdateInterviewAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await ProxyHelper.ProxyResponse(await _client.DeleteInterviewAsync(id));

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetByCandidate(int candidateId)
        => await ProxyHelper.ProxyResponse(await _client.GetInterviewsByCandidateAsync(candidateId));

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
        => await ProxyHelper.ProxyResponse(await _client.GetInterviewCountAsync());
}