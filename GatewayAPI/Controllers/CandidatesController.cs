using CandidateService.DTOs;
using GatewayAPI.DTOs.Candidates;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("internal/api/candidates")]
[ExcludeFromCodeCoverage]
public class CandidatesController : ControllerBase
{
    private readonly CandidateClient _client;

    public CandidatesController(CandidateClient client)
    {
        _client = client;
    }

    // CREATE 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DTOs.Candidates.CreateCandidateRequests request)
    {
        var response = await _client.CreateAsync(request);
        return await ProxyResponse(response);
    }

    // GET ALL (PAGINATED) 
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var response = await _client.GetAllAsync(page, pageSize);
        return await ProxyResponse(response);
    }

    // GET BY ID 
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _client.GetByIdAsync(id);
        return await ProxyResponse(response);
    }

    // UPDATE 
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DTOs.Candidates.CreateCandidateRequests request)
    {
        var response = await _client.UpdateAsync(id, request);
        return await ProxyResponse(response);
    }

    // DELETE 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _client.DeleteAsync(id);
        return await ProxyResponse(response);
    }

    // BULK UPLOAD (JSON / CSV) 
    [HttpPost("bulk")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> BulkUpload([FromForm] BulkCandidateForm form)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest("File is missing.");

        var response = await _client.BulkUploadAsync(form.File);
        return await ProxyResponse(response);
    }


    //Used to generate proper response body
    private static async Task<IActionResult> ProxyResponse(HttpResponseMessage response)
    {
        // No body (e.g., DELETE 204)
        if (response.Content == null)
            return new StatusCodeResult((int)response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.MediaType;

        // Try JSON first
        if (contentType != null && contentType.Contains("application/json"))
        {
            var body = await response.Content.ReadFromJsonAsync<object>();

            if (!response.IsSuccessStatusCode)
                return new ObjectResult(body)
                {
                    StatusCode = (int)response.StatusCode
                };

            return new ObjectResult(body)
            {
                StatusCode = (int)response.StatusCode
            };
        }

        // plain text, etc.
        var text = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new ObjectResult(text)
            {
                StatusCode = (int)response.StatusCode
            };

        return new ObjectResult(text)
        {
            StatusCode = (int)response.StatusCode
        };
    }
}
