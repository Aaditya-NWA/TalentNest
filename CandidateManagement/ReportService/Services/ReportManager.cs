using ReportService.DTOs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReportService.Services;

[ExcludeFromCodeCoverage]
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

    public async Task<ReportSummaryResponse> GetSystemSummaryAsync()
    {
        var candidateTask = _candidates.GetCountsAsync();
        var interviewTask = _interviews.GetCountsAsync();
        var requirementTask = _requirements.GetCountsAsync();

        await Task.WhenAll(candidateTask, interviewTask, requirementTask);

        return new ReportSummaryResponse
        {
            TotalCandidates = candidateTask.Result.Total,
            TotalInterviews = interviewTask.Result.Total,
            ScheduledInterviews = interviewTask.Result.Scheduled,
            OpenRequirements = requirementTask.Result.Open
        };
    }

    public async Task<CandidateReportResponse> GetCandidateReportAsync()
    {
        var candidatesTask = _candidates.GetAllAsync();
        var interviewsTask = _interviews.GetAllAsync();

        await Task.WhenAll(
            candidatesTask,
            interviewsTask);

        var candidates = candidatesTask.Result;
        var interviews = interviewsTask.Result;

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var blocked =
            interviews
            .Where(i => i.InterviewDate >= sixMonthsAgo)
            .Select(i => i.CandidateId)
            .Distinct()
            .ToList();

        var skills =
            candidates
            .SelectMany(c =>
                (c.SkillSet ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()))
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());

        var proficiency =
            candidates
            .GroupBy(x =>
                string.IsNullOrWhiteSpace(x.PrimarySkillLevel)
                ? "Unknown"
                : x.PrimarySkillLevel)
            .ToDictionary(x => x.Key, x => x.Count());

        var available =
            candidates.Count(x =>
                x.AvailabilityDate <= DateTime.UtcNow);

        var percent =
            candidates.Count == 0
                ? 0
                : (double)available /
                  candidates.Count * 100;

        return new CandidateReportResponse
        {
            TotalCandidates = candidates.Count,
            SkillsDistribution = skills,
            ProficiencyDistribution = proficiency,
            AvailableCandidates = available,
            AvailabilityPercentage = Math.Round(percent, 2),
            BlockedCandidatesLast6Months = blocked
        };
    }

    public async Task<InterviewValidationReportResponse>
        GetInterviewValidationReportAsync()
    {
        var interviews =
            await _interviews.GetAllAsync();

        var sixMonthsAgo =
            DateTime.UtcNow.AddMonths(-6);

        var blocked =
            interviews
            .Where(i => i.InterviewDate >= sixMonthsAgo)
            .GroupBy(i =>
                new { i.CandidateId, i.Project })
            .Select(g =>
            {
                var latest =
                    g.OrderByDescending(x =>
                        x.InterviewDate)
                    .First();

                var eligible =
                    latest.InterviewDate
                    .AddMonths(6);

                var days =
                    (eligible -
                     DateTime.UtcNow).Days;

                return new BlockedCandidateInfo
                {
                    CandidateId = g.Key.CandidateId,
                    Project = g.Key.Project,
                    LastInterviewDate =
                        latest.InterviewDate,
                    DaysUntilEligible =
                        Math.Max(days, 0)
                };
            })
            .ToList();

        return new InterviewValidationReportResponse
        {
            BlockedCandidates = blocked
        };
    }

    public async Task<RequirementFulfillmentReportResponse>
    GetRequirementFulfillmentReportAsync()
        {
            var paged =
                await GetRequirementFulfillmentPagedAsync(1, int.MaxValue);

            return new RequirementFulfillmentReportResponse
            {
                Requirements = paged.Requirements
            };
        }

    public async Task<OutcomeReportResponse>
        GetOutcomeReportAsync()
    {
        var interviews =
            await _interviews.GetAllAsync();

        var breakdown =
            interviews
            .GroupBy(i =>
                string.IsNullOrWhiteSpace(
                    i.FinalOutcome)
                ? "Unknown"
                : i.FinalOutcome)
            .ToDictionary(
                x => x.Key,
                x => x.Count());

        return new OutcomeReportResponse
        {
            TotalInterviews =
                interviews.Count,
            OutcomeBreakdown =
                breakdown
        };
    }

    public async Task<PerformanceReportResponse> RunPerformanceTestAsync(int requestCount)
    {
        if (requestCount <= 0)
            throw new ArgumentException("Request count must be positive.");

        var page = await _requirements.GetPageAsync(1, 1);

        if (!page.Data.Any())
            throw new InvalidOperationException(
                "No requirements available for performance test.");

        var requirementId = page.Data.First().Id;

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

    public async Task<CandidateDetailedReportResponse?>
        GetCandidateDetailedReportAsync(int id)
    {
        var candidate =
            await _candidates.GetByIdAsync(id);

        if (candidate == null)
            return null;

        var interviews =
            await _interviews
            .GetByCandidateAsync(id);

        var requirementPage =
            await _requirements.GetPageAsync(1, int.MaxValue);

        var requirements = requirementPage.Data;

        return new CandidateDetailedReportResponse
        {
            Candidate = candidate,
            Interviews = interviews,
            Requirements =
                requirements
                .Select(r =>
                    new RequirementSummaryDto
                    {
                        RequirementId = r.Id,
                        Project = r.Project,
                        Matched =
                            candidate.SkillSet
                            .Contains(
                                r.SkillsNeeded),
                        Interviewed =
                            interviews.Any(i =>
                                i.Project ==
                                r.Project),
                        Selected =
                            interviews.Any(i =>
                                i.Project ==
                                r.Project &&
                                i.FinalOutcome ==
                                "Selected")
                    })
                .ToList(),
            BlockedBySixMonthRule =
                interviews.Any(i =>
                    i.InterviewDate >=
                    DateTime.UtcNow
                    .AddMonths(-6))
        };
    }
    public async Task<CandidateReportPagedResponse>
    GetCandidateReportPagedAsync(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Invalid pagination parameters");

        var candidatePage =
            await _candidates.GetPageAsync(page, pageSize);

        var interviews =
            await _interviews.GetAllAsync();

        var candidates = candidatePage.Data;
        var totalCandidates = candidatePage.TotalCount;

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var blocked =
            interviews
                .Where(i => i.InterviewDate >= sixMonthsAgo)
                .Select(i => i.CandidateId)
                .Distinct()
                .ToList();

        var skills =
            candidates
                .SelectMany(c =>
                    (c.SkillSet ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()))
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

        var proficiency =
            candidates
                .GroupBy(c =>
                    string.IsNullOrWhiteSpace(c.PrimarySkillLevel)
                        ? "Unknown"
                        : c.PrimarySkillLevel)
                .ToDictionary(x => x.Key, x => x.Count());

        var available =
            candidates.Count(c =>
                c.AvailabilityDate <= DateTime.UtcNow);

        var percent =
            candidates.Count == 0
                ? 0
                : (double)available / candidates.Count * 100;

        return new CandidateReportPagedResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCandidates = totalCandidates,
            TotalPages = candidatePage.TotalPages,

            SkillsDistribution = skills,
            ProficiencyDistribution = proficiency,

            AvailableCandidates = available,
            AvailabilityPercentage = Math.Round(percent, 2),
            BlockedCandidatesLast6Months = blocked
        };
    }
    public async Task<RequirementFulfillmentPagedResponse>
    GetRequirementFulfillmentPagedAsync(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Invalid pagination parameters");

        var requirementPage =
            await _requirements.GetPageAsync(page, pageSize);

        var candidates = await _candidates.GetAllAsync();
        var interviews = await _interviews.GetAllAsync();

        var result = new List<RequirementFulfillmentInfo>();

        foreach (var requirement in requirementPage.Data)
        {
            var matchedCandidates = candidates
                .Where(c =>
                {
                    var candidateSkills = (c.SkillSet ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());

                    var requirementSkills = (requirement.SkillsNeeded ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());

                    var hasSkills =
                        requirementSkills.All(skill =>
                            candidateSkills.Contains(skill));

                    var experienceMatch =
                        c.ExperienceMonths >= requirement.MinExperienceMonths &&
                        c.ExperienceMonths <= requirement.MaxExperienceMonths;

                    var primarySkillMatch =
                        string.IsNullOrWhiteSpace(requirement.RequiredPrimarySkillLevel)
                        || c.PrimarySkillLevel == requirement.RequiredPrimarySkillLevel;

                    return hasSkills && experienceMatch && primarySkillMatch;
                })
                .ToList();

            var interviewedCandidates = interviews
                .Where(i => i.Project == requirement.Project)
                .Select(i => i.CandidateId)
                .Distinct()
                .Count();

            var selectedCandidates = interviews
                .Where(i =>
                    i.Project == requirement.Project &&
                    i.FinalOutcome == "Selected")
                .Select(i => i.CandidateId)
                .Distinct()
                .Count();

            var fulfillmentPercentage =
                matchedCandidates.Count == 0
                    ? 0
                    : (double)selectedCandidates / matchedCandidates.Count * 100;

            result.Add(new RequirementFulfillmentInfo
            {
                RequirementId = requirement.Id,
                Project = requirement.Project,
                MatchedCandidates = matchedCandidates.Count,
                InterviewedCandidates = interviewedCandidates,
                SelectedCandidates = selectedCandidates,
                FulfillmentPercentage = Math.Round(fulfillmentPercentage, 2)
            });
        }

        return new RequirementFulfillmentPagedResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalRequirements = requirementPage.TotalCount,
            TotalPages = requirementPage.TotalPages,
            Requirements = result
        };
    }
}