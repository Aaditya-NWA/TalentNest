using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Contracts.Services;

public interface IRequirementService
{
    [ExcludeFromCodeCoverage]
    // CREATE
    Task<RequirementResponse> CreateRequirementAsync(CreateRequirementRequest request);
    // READ
    Task<RequirementResponse?> GetRequirementByIdAsync(int id);
    Task<IEnumerable<RequirementResponse>> GetAllRequirementsAsync();
    // UPDATE
    Task<RequirementResponse?> UpdateRequirementAsync(int id, CreateRequirementRequest request);

}