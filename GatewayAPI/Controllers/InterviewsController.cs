using InterviewService.DTOs.Requests.Interviews;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

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
    public async Task<IActionResult> Create(CreateInterviewRequest request)
        => await ProxyResponse(await _client.CreateAsync(request));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => await ProxyResponse(await _client.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => await ProxyResponse(await _client.GetByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateInterviewRequest request)
        => await ProxyResponse(await _client.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await ProxyResponse(await _client.DeleteAsync(id));

    private static async Task<IActionResult> ProxyResponse(HttpResponseMessage response)
    {
        if (response.Content == null)
            return new StatusCodeResult((int)response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<object>();

        return new ObjectResult(body)
        {
            StatusCode = (int)response.StatusCode
        };
    }
}
