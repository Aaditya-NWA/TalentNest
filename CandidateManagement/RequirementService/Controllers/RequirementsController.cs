using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.DTOs;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Models;
using RequirementService.Services;
using System.Diagnostics.CodeAnalysis;


namespace RequirementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly IRequirementService _requirementService;
        private readonly RequirementDbContext _context;

        private readonly IMatchingService _matchingService;

        public RequirementsController(RequirementDbContext context, IMatchingService matchingService, IRequirementService requirementService)
        {
            _context = context;
            _matchingService = matchingService;
            _requirementService = requirementService;

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be negative");

            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
                return NotFound();

            return Ok(requirement);
        }
        [ExcludeFromCodeCoverage]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 50)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters");

            var totalCount =
                await _context.Requirements.CountAsync();

            var totalPages =
                (int)Math.Ceiling(totalCount / (double)pageSize);

            var data = await _context.Requirements
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PaginatedRequirementResponse
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            });
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
                CreatedAt = DateTime.UtcNow,
                ClientInterviewRequired = dto.ClientInterviewRequired
            };

            _context.Requirements.Add(requirement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), 
                new { id = requirement.Id }, requirement);
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateRequirementRequest request)
        {
            try
            {
                var updated = await _requirementService.UpdateRequirementAsync(id, request);

                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be negative");

            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
                return NotFound();

            _context.Requirements.Remove(requirement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/match")]
        public async Task<IActionResult> MatchCandidates(int id)
        {
            if (id <= 0)
                return BadRequest("Id cannot be negative or zero");

            try
            {
                var matches = await _matchingService.GetRankedMatchesAsync(id);
                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

        }
        [ExcludeFromCodeCoverage]
        [HttpGet("count")]
        public async Task<IActionResult> GetRequirementCounts()
        {
            var now = DateTime.UtcNow;

            var open = await _context.Requirements
                .CountAsync(r =>
                    r.AvailabilityStart <= now &&
                    r.AvailabilityEnd >= now);

            return Ok(new RequirementCountResponse
            {
                Open = open
            });
        }
    }
}
