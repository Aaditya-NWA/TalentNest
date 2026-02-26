using System.Diagnostics.CodeAnalysis;

namespace RequirementService.DTOs;

[ExcludeFromCodeCoverage]
public class RequirementCountResponse
{
    public int Open { get; set; }
}