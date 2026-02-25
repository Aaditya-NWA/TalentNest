using Microsoft.AspNetCore.Mvc;
using RequirementService.Contracts.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Controllers
{
    
    [ApiController]
    [Route("api/performance")]
    [ExcludeFromCodeCoverage]
    public class PerformanceController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public PerformanceController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpGet("p95/{requirementId}")]
        public async Task<IActionResult> MeasureP95(int requirementId)
        {
            if (requirementId <= 0)
                return BadRequest("Invalid requirementId");

            var timings = new List<long>();

            for (int i = 0; i < 100; i++)
            {
                var sw = Stopwatch.StartNew();
                await _matchingService.GetRankedMatchesAsync(requirementId);
                sw.Stop();

                timings.Add(sw.ElapsedMilliseconds);
            }

            timings.Sort();
            int index95 = (int)(0.95 * timings.Count) - 1;

            return Ok(new
            {
                Runs = 100,
                P95 = timings[index95],
                Min = timings.First(),
                Max = timings.Last(),
                Avg = timings.Average()
            });
        }
    }
}