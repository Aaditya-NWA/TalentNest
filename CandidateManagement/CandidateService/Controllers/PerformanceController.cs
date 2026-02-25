using CandidateService.Data;
using CandidateService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Controllers
{
    [ApiController]
    [Route("api/performance")]
    [ExcludeFromCodeCoverage]
    public class PerformanceController : ControllerBase
    {
        private readonly CandidateDbContext _context;

        public PerformanceController(CandidateDbContext context)
        {
            _context = context;
        }

        [HttpPost("seed/{count}")]
        [ExcludeFromCodeCoverage]
        public async Task<IActionResult> Seed(int count = 10000)
        {
            if (_context.Candidates.Count() >= count)
                return Ok("Already seeded");

            var list = new List<Candidate>();

            for (int i = 0; i < count; i++)
            {
                list.Add(new Candidate
                {
                    Name = $"Candidate_{i}",
                    SkillSet = "C#,SQL,Azure",
                    ExperienceMonths = Random.Shared.Next(12, 180),
                    AvailabilityDate = DateTime.UtcNow.AddDays(Random.Shared.Next(0, 90))
                });
            }

            _context.Candidates.AddRange(list);
            await _context.SaveChangesAsync();

            return Ok($"Seeded {count} candidates");
        }
    }

}