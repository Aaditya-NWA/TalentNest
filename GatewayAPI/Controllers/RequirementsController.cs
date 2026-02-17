using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using RequirementService.DTOs.Requests;
using System.Diagnostics.CodeAnalysis;

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
    public async Task<IActionResult> Create(CreateRequirementRequest request)
        => await ProxyResponse(await _client.CreateAsync(request));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => await ProxyResponse(await _client.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => await ProxyResponse(await _client.GetByIdAsync(id));

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
