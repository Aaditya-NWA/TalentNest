using Microsoft.EntityFrameworkCore;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Models;
using System.Diagnostics.CodeAnalysis;

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
        var requirement = new Requirement
        {
            Project = request.Project,
            SkillsNeeded = request.SkillsNeeded,
            ExperienceMonths = request.ExperienceMonths,
            AvailabilityWindow = request.AvailabilityWindow,
            ClientInterviewRequired = request.ClientInterviewRequired
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
            ExperienceMonths = requirement.ExperienceMonths,
            AvailabilityWindow = requirement.AvailabilityWindow,
            ClientInterviewRequired = requirement.ClientInterviewRequired,
            CreatedAt = requirement.CreatedAt
        };
    }
}