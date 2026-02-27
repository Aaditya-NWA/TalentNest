using GatewayAPI.DTOs.Candidates;
using GatewayAPI.Helpers;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/candidates")]
[ExcludeFromCodeCoverage]
public class CandidatesController : ControllerBase
{
    private readonly CandidateClient _client;

    public CandidatesController(CandidateClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCandidateRequests request)
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
    public async Task<IActionResult> Update(int id, [FromBody] CreateCandidateRequests request)
        => await ProxyHelper.ProxyResponse(await _client.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await ProxyHelper.ProxyResponse(await _client.DeleteAsync(id));

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? skill,
        [FromQuery] int? minExp,
        [FromQuery] int? maxExp,
        [FromQuery] string? primarySkillLevel,
        [FromQuery] string? start,
        [FromQuery] string? end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await ProxyHelper.ProxyResponse(
            await _client.SearchAsync(skill, minExp, maxExp, primarySkillLevel, page, pageSize, start, end));

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
        => await ProxyHelper.ProxyResponse(await _client.GetCountAsync());

    [HttpPost("bulk")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> BulkUpload([FromForm] BulkCandidateForm form)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest("File is missing.");

        return await ProxyHelper.ProxyResponse(await _client.BulkUploadAsync(form.File));
    }
}