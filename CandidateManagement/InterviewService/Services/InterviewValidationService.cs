using CandidateService.Data;
using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.Data;
using InterviewService.Models;
using InterviewService.Models.Enums;
using Microsoft.EntityFrameworkCore;
using RequirementService.Data;

namespace InterviewService.Services;

public class InterviewValidationService : IInterviewValidationService
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly CandidateDbContext _candidateDbContext; // For Candidates
    private readonly RequirementDbContext _requirementDbContext; // For Requirements

    public InterviewValidationService(
        IInterviewRepository interviewRepository,
        CandidateDbContext candidateDbContext,
        RequirementDbContext requirementDbContext) // Add RequirementDbContext
    {
        _interviewRepository = interviewRepository;
        _candidateDbContext = candidateDbContext;
        _requirementDbContext = requirementDbContext;
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateInterviewAsync(Interview interview)
    {
        // ============ CHECK 1: Candidate Existence ============
        var candidateExists = await _candidateDbContext.Candidates
            .AnyAsync(c => c.Id == interview.CandidateId);

        if (!candidateExists)
        {
            return (false, $"Candidate with ID {interview.CandidateId} does not exist in the system.");
        }

        // ============ CHECK 2: Requirement Existence and Get ClientInterviewRequired ============
        var requirement = await _requirementDbContext.Requirements
                            .Where(r => r.Id == interview.RequirementId &&
                                        r.Project.ToUpper() == interview.Project.ToUpper())  // Match both RequirementId AND Project (case-insensitive)
                            .Select(r => new { r.ClientInterviewRequired })
                            .FirstOrDefaultAsync();

        if (requirement == null)
        {
            return (false, $"Requirement ID {interview.RequirementId} does not exist in the for the project {interview.Project}.");
        }

        // Override the interview's ClientInterviewRequired with the value from requirement
        bool CIRequired = requirement.ClientInterviewRequired;

        // ============ CHECK 3: same date and 6-month rule ============
        var scheduleValidation = await HasInterviewedInLastSixMonthsAsync(
            interview.CandidateId,
            interview.Project,
            interview.InterviewDate);

        if (!scheduleValidation.IsValid)
        {
            return (false, scheduleValidation.ErrorMessage);
        }

        // ============ CHECK 4: Interview level validation ============
        var isValidLevel = await ValidateInterviewLevelAsync(
            (int)interview.Level,
            CIRequired); // Now uses the value from requirement

        if (!isValidLevel)
        {
            return (false,
                $"Invalid interview level. Client interview requires ClientInterviewRequired = true. " +
                $"Requirement Id {interview.RequirementId} has ClientInterviewRequired = {CIRequired}.");
        }

        return (true, string.Empty);
    }

    public async Task<bool> ValidateInterviewLevelAsync(int level, bool clientInterviewRequired)
    {
        if (level == (int)InterviewLevel.Client && !clientInterviewRequired)
            return false;

        return Enum.IsDefined(typeof(InterviewLevel), level);
    }

    public async Task<(bool IsValid, string ErrorMessage)> HasInterviewedInLastSixMonthsAsync(int candidateId, string project, DateTime interviewDate)
    {
        return await _interviewRepository.HasInterviewedInLastSixMonthsAsync(candidateId, project, interviewDate);
    }
}