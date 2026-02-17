using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementService.Data;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Models;

namespace RequirementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly RequirementDbContext _context;

        public RequirementsController(RequirementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Requirements.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
                return NotFound();

            return Ok(requirement);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRequirementRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ExperienceRange))
                return BadRequest("ExperienceRange is required in format: min,max");

            if (string.IsNullOrWhiteSpace(dto.AvailabilityWindow))
                return BadRequest("AvailabilityWindow is required in format: start,end");

            // ---- Parse Experience Range ----
            var expParts = dto.ExperienceRange.Split(',');

            if (expParts.Length != 2 ||
                string.IsNullOrWhiteSpace(expParts[0]) ||
                string.IsNullOrWhiteSpace(expParts[1]))
            {
                return BadRequest("ExperienceRange must contain two values: min,max");
            }

            if (!int.TryParse(expParts[0], out int minExp) ||
                !int.TryParse(expParts[1], out int maxExp))
            {
                return BadRequest("ExperienceRange must contain valid integers");
            }

            if (minExp < 0 || maxExp < 0)
                return BadRequest("Experience values cannot be negative");

            if (minExp >= maxExp)
                return BadRequest("Min experience must be less than max experience");

            // ---- Parse Availability Window ----
            var availParts = dto.AvailabilityWindow.Split(',');

            if (availParts.Length != 2 ||
                string.IsNullOrWhiteSpace(availParts[0]) ||
                string.IsNullOrWhiteSpace(availParts[1]))
            {
                return BadRequest("AvailabilityWindow must contain two values: start,end");
            }

            if (!DateTime.TryParse(availParts[0], out DateTime startDate) ||
                !DateTime.TryParse(availParts[1], out DateTime endDate))
            {
                return BadRequest("AvailabilityWindow must contain valid dates");
            }

            if (startDate > endDate)
                return BadRequest("Availability start cannot be after end date");

            // ---- Create Entity ----
            var requirement = new Requirement
            {
                Project = dto.Project,
                SkillsNeeded = dto.SkillsNeeded,
                MinExperienceMonths = minExp,
                MaxExperienceMonths = maxExp,
                AvailabilityStart = startDate,
                AvailabilityEnd = endDate,
                ClientInterviewRequired = dto.ClientInterviewRequired
            };

            _context.Requirements.Add(requirement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), 
                new { id = requirement.Id }, requirement);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
                return NotFound();

            _context.Requirements.Remove(requirement);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
