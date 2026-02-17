using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/reports")]
[ExcludeFromCodeCoverage]
public class ReportsController : ControllerBase
{
    private readonly ReportClient _client;

    public ReportsController(ReportClient client)
    {
        _client = client;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var report = await _client.GetSummaryAsync();

        if (report == null)
            return StatusCode(502, "Report service unavailable");

        return Ok(report);
    }
}

//using GatewayAPI.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics.CodeAnalysis;

//namespace GatewayAPI.Controllers;

//[ApiController]
//[Route("api/reports")]
//[ExcludeFromCodeCoverage]
//public class ReportsController : ControllerBase
//{
//    private readonly ReportClient _client;

//    public ReportsController(ReportClient client)
//    {
//        _client = client;
//    }

//    [HttpGet("summary")]
//    public async Task<IActionResult> GetSummary()
//    {
//        var report = await _client.GetSummaryAsync();

//        if (report == null)
//            return StatusCode(502, "Report service unavailable");

//        return Ok(report);
//    }
//}


