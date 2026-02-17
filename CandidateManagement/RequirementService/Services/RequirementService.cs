using Microsoft.EntityFrameworkCore;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.DTOs;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Models;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace RequirementService.Services;

[ExcludeFromCodeCoverage]
public class RequirementService : IRequirementService
{
    private readonly RequirementDbContext _context;

    public RequirementService(RequirementDbContext context)
    {
        _context = context;
    }

    // ============ CREATE ============
    public async Task<RequirementResponse> CreateRequirementAsync(CreateRequirementRequest request)
    {
        // -------- Validate Experience Range --------
        if (string.IsNullOrWhiteSpace(request.ExperienceRange))
            throw new ArgumentException("ExperienceRange is required in format: min,max");

        var expParts = request.ExperienceRange.Split(',');

        if (expParts.Length != 2 ||
            string.IsNullOrWhiteSpace(expParts[0]) ||
            string.IsNullOrWhiteSpace(expParts[1]))
            throw new ArgumentException("ExperienceRange must contain two values: min,max");

        if (!int.TryParse(expParts[0], out int minExp) ||
            !int.TryParse(expParts[1], out int maxExp))
            throw new ArgumentException("ExperienceRange must contain valid integers");

        if (minExp < 0 || maxExp < 0)
            throw new ArgumentException("Experience values cannot be negative");

        if (minExp >= maxExp)
            throw new ArgumentException("Min experience must be less than max experience");

        // -------- Validate Availability Window --------
        if (string.IsNullOrWhiteSpace(request.AvailabilityWindow))
            throw new ArgumentException("AvailabilityWindow is required in format: start,end");

        var availParts = request.AvailabilityWindow.Split(',');

        if (availParts.Length != 2 ||
            string.IsNullOrWhiteSpace(availParts[0]) ||
            string.IsNullOrWhiteSpace(availParts[1]))
            throw new ArgumentException("AvailabilityWindow must contain two values: start,end");

        if (!DateTime.TryParse(availParts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
            !DateTime.TryParse(availParts[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            throw new ArgumentException("AvailabilityWindow must contain valid dates");

        if (startDate >= endDate)
            throw new ArgumentException("Availability start must be earlier than end date");

        // -------- Create Entity --------
        var requirement = new Requirement
        {
            Project = request.Project,
            SkillsNeeded = request.SkillsNeeded,
            MinExperienceMonths = minExp,
            MaxExperienceMonths = maxExp,
            AvailabilityStart = startDate,
            AvailabilityEnd = endDate,
            ClientInterviewRequired = request.ClientInterviewRequired,
            CreatedAt = DateTime.UtcNow
        };

        _context.Requirements.Add(requirement);
        await _context.SaveChangesAsync();

        return MapToResponse(requirement);
    }

    // ============ READ ============
    public async Task<RequirementResponse?> GetRequirementByIdAsync(int id)
    {
        var requirement = await _context.Requirements.FindAsync(id);
        return requirement == null ? null : MapToResponse(requirement);
    }

    public async Task<IEnumerable<RequirementResponse>> GetAllRequirementsAsync()
    {
        var requirements = await _context.Requirements
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requirements.Select(MapToResponse);
    }

    // ============ PRIVATE ============
    private static RequirementResponse MapToResponse(Requirement requirement)
    {
        return new RequirementResponse
        {
            Id = requirement.Id,
            Project = requirement.Project,
            SkillsNeeded = requirement.SkillsNeeded,
            MinExperienceMonths = requirement.MinExperienceMonths,
            MaxExperienceMonths = requirement.MaxExperienceMonths,
            AvailabilityStart = requirement.AvailabilityStart,
            AvailabilityEnd = requirement.AvailabilityEnd,
            ClientInterviewRequired = requirement.ClientInterviewRequired,
            CreatedAt = requirement.CreatedAt
        };
    }
}
