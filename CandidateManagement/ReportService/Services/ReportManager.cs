using ReportService.DTOs;
using System.Diagnostics;

namespace ReportService.Services;

public class ReportManager : IReportManager
{
    private readonly CandidateClient _candidates;
    private readonly IInterviewClient _interviews;
    private readonly RequirementClient _requirements;


    public ReportManager(
      CandidateClient candidates,
      IInterviewClient interviews,
      RequirementClient requirements)
    {
        _candidates = candidates;
        _interviews = interviews;
        _requirements = requirements;
    }

    // ----------------------------------------------------
    // SUMMARY REPORT
    // ----------------------------------------------------
    public async Task<ReportSummaryResponse> GetSystemSummaryAsync()
    {
        var candidates = await _candidates.GetAllAsync();
        var requirements = await _requirements.GetAllAsync();

        var allInterviews = await _interviews.GetAllAsync();


        return new ReportSummaryResponse
        {
            TotalCandidates = candidates.Count,
            TotalInterviews = allInterviews.Count,
            ScheduledInterviews =
                allInterviews.Count(i => i.Status == "Scheduled"),
            OpenRequirements =
                requirements.Count(r => r.Status == "Open")
        };
    }

    // ----------------------------------------------------
    // CANDIDATE REPORT
    // ----------------------------------------------------
    public async Task<CandidateReportResponse> GetCandidateReportAsync()
    {
        var candidates = await _candidates.GetAllAsync();

        var allInterviews = await _interviews.GetAllAsync();


        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var blockedCandidates = allInterviews
            .Where(i => i.InterviewDate >= sixMonthsAgo)
            .Select(i => i.CandidateId)
            .Distinct()
            .ToList();

        // Skills distribution (split comma-separated skills)
        var skillsDistribution = candidates
            .SelectMany(c =>
                (c.SkillSet ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()))
            .GroupBy(skill => skill)
            .ToDictionary(g => g.Key, g => g.Count());

        // Proficiency distribution (PrimarySkillLevel)
        var proficiencyDistribution = candidates
            .GroupBy(c => string.IsNullOrWhiteSpace(c.PrimarySkillLevel)
                ? "Unknown"
                : c.PrimarySkillLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        // Availability logic (available if availabilityDate <= today)
        var availableCount = candidates
            .Count(c => c.AvailabilityDate <= DateTime.UtcNow);

        var availabilityPercentage =
            candidates.Count == 0
                ? 0
                : (double)availableCount / candidates.Count * 100;

        return new CandidateReportResponse
        {
            TotalCandidates = candidates.Count,
            SkillsDistribution = skillsDistribution,
            ProficiencyDistribution = proficiencyDistribution,
            AvailableCandidates = availableCount,
            AvailabilityPercentage = Math.Round(availabilityPercentage, 2),
            BlockedCandidatesLast6Months = blockedCandidates
        };
    }

    // ----------------------------------------------------
    // INTERVIEW VALIDATION REPORT (6-Month Rule)
    // ----------------------------------------------------
    public async Task<InterviewValidationReportResponse> GetInterviewValidationReportAsync()
    {
        var candidates = await _candidates.GetAllAsync();

        var allInterviews = await _interviews.GetAllAsync();


        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var blocked = allInterviews
            .Where(i => i.InterviewDate >= sixMonthsAgo)
            .GroupBy(i => new { i.CandidateId, i.Project })
            .Select(g =>
            {
                var latestInterview = g
                    .OrderByDescending(x => x.InterviewDate)
                    .First();

                var eligibleDate = latestInterview.InterviewDate.AddMonths(6);

                var daysRemaining =
                    (eligibleDate - DateTime.UtcNow).Days;

                return new BlockedCandidateInfo
                {
                    CandidateId = g.Key.CandidateId,
                    Project = g.Key.Project,
                    LastInterviewDate = latestInterview.InterviewDate,
                    DaysUntilEligible =
                        daysRemaining > 0 ? daysRemaining : 0
                };
            })
            .ToList();

        return new InterviewValidationReportResponse
        {
            BlockedCandidates = blocked
        };
    }

    // ----------------------------------------------------
    // REQUIREMENT FULFILLMENT REPORT
    // ----------------------------------------------------
    public async Task<RequirementFulfillmentReportResponse>
        GetRequirementFulfillmentReportAsync()
    {
        var candidates = await _candidates.GetAllAsync();
        var requirements = await _requirements.GetAllAsync();

        var allInterviews = await _interviews.GetAllAsync();


        var result = new List<RequirementFulfillmentInfo>();

        foreach (var requirement in requirements)
        {
            var matchedCandidates = candidates
    .Where(c =>
    {
        var candidateSkills = (c.SkillSet ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        var requirementSkills = (requirement.SkillsNeeded ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        var hasSkills =
            requirementSkills.All(skill =>
                candidateSkills.Contains(skill));

        var experienceMatch =
            c.ExperienceMonths >= requirement.MinExperienceMonths &&
            c.ExperienceMonths <= requirement.MaxExperienceMonths;

        var primarySkillMatch =
            string.IsNullOrWhiteSpace(requirement.RequiredPrimarySkillLevel)
            || c.PrimarySkillLevel == requirement.RequiredPrimarySkillLevel;

        return hasSkills &&
               experienceMatch &&
               primarySkillMatch;
    })
    .ToList();


            var interviewedCandidates = allInterviews
                .Where(i => i.Project == requirement.Project)
                .Select(i => i.CandidateId)
                .Distinct()
                .Count();

            var selectedCandidates = allInterviews
                .Where(i =>
                    i.Project == requirement.Project &&
                    i.Outcome == "Selected")
                .Select(i => i.CandidateId)
                .Distinct()
                .Count();

            var fulfillmentPercentage =
                matchedCandidates.Count == 0
                    ? 0
                    : (double)selectedCandidates /
                      matchedCandidates.Count * 100;

            result.Add(new RequirementFulfillmentInfo
            {
                RequirementId = requirement.Id,
                Project = requirement.Project,
                MatchedCandidates = matchedCandidates.Count,
                InterviewedCandidates = interviewedCandidates,
                SelectedCandidates = selectedCandidates,
                FulfillmentPercentage =
                    Math.Round(fulfillmentPercentage, 2)
            });
        }

        return new RequirementFulfillmentReportResponse
        {
            Requirements = result
        };
    }

    // ----------------------------------------------------
    // OUTCOME REPORT
    // ----------------------------------------------------
    public async Task<OutcomeReportResponse> GetOutcomeReportAsync()
    {
        var candidates = await _candidates.GetAllAsync();

        var allInterviews = await _interviews.GetAllAsync();


        var breakdown = allInterviews
            .GroupBy(i =>
                string.IsNullOrWhiteSpace(i.Outcome)
                    ? "Unknown"
                    : i.Outcome)
            .ToDictionary(g => g.Key, g => g.Count());

        return new OutcomeReportResponse
        {
            TotalInterviews = allInterviews.Count,
            OutcomeBreakdown = breakdown
        };
    }

    public async Task<PerformanceReportResponse> RunPerformanceTestAsync(int requestCount)
    {
        if (requestCount <= 0)
            throw new ArgumentException("Request count must be positive.");

        var requirements = await _requirements.GetAllAsync();

        if (!requirements.Any())
            throw new InvalidOperationException("No requirements available for performance test.");

        var requirementId = requirements.First().Id;
            
        var latencies = new List<double>();

        // Warm-up call
        await _requirements.MatchAsync(requirementId);


        for (int i = 0; i < requestCount; i++)
        {
            var sw = Stopwatch.StartNew();

            await _requirements.MatchAsync(requirementId);

            sw.Stop();

            latencies.Add(sw.Elapsed.TotalMilliseconds);
        }

        latencies.Sort();

        var average = latencies.Average();
        var min = latencies.First();
        var max = latencies.Last();

        var p95Index = (int)Math.Ceiling(0.95 * latencies.Count) - 1;
        p95Index = Math.Clamp(p95Index, 0, latencies.Count - 1);

        var p95 = latencies[p95Index];

        return new PerformanceReportResponse
        {
            TotalRequests = requestCount,
            AverageLatencyMs = Math.Round(average, 2),
            MinLatencyMs = Math.Round(min, 2),
            MaxLatencyMs = Math.Round(max, 2),
            P95LatencyMs = Math.Round(p95, 2)
        };
    }
}




