using CandidateService.Models;
using Microsoft.Extensions.Caching.Distributed;
using ReportService.DTOs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ReportService.Services;

[ExcludeFromCodeCoverage]
public class ReportManager : IReportManager
{
    private readonly CandidateClient _candidates;
    private readonly IInterviewClient _interviews;
    private readonly RequirementClient _requirements;
    private readonly IDistributedCache _cache;

    public ReportManager(
        CandidateClient candidates,
        IInterviewClient interviews,
        RequirementClient requirements,
        IDistributedCache cache)
    {
        _candidates = candidates;
        _interviews = interviews;
        _requirements = requirements;
        _cache = cache;
    }

    private static readonly JsonSerializerOptions CacheJsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    // ════════════════════════════════════════════════════════════════════════════
    //  MERGED CANDIDATE REPORT
    //  Combines: Summary counts + paged candidate stats + paged all-candidate details
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Single endpoint combining:
    ///   1. System summary (counts)
    ///   2. Paged candidate report (skills / proficiency / availability)
    ///   3. Paginated all-candidates detailed report (per-candidate interviews + requirements)
    /// All three sections are fetched in parallel.
    /// </summary>
    public async Task<CombinedCandidateReportResponse> GetCombinedCandidateReportAsync(
        int page,
        int pageSize,
        int detailPage,
        int detailPageSize)
    {
        var summaryTask = GetSystemSummaryAsync();
        var pagedTask = GetCandidateReportPagedAsync(page, pageSize);
        var allDetailedTask = GetAllCandidatesDetailedReportAsync(detailPage, detailPageSize);

        await Task.WhenAll(summaryTask, pagedTask, allDetailedTask);

        return new CombinedCandidateReportResponse
        {
            Summary = summaryTask.Result,
            CandidateStats = pagedTask.Result,
            AllCandidateDetails = allDetailedTask.Result
        };
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  ALL-CANDIDATES DETAILED REPORT  (same logic as single-candidate but paged)
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns detailed per-candidate info (interviews + requirement matches) for
    /// every candidate on the requested page. Candidates and interviews are loaded
    /// from cache so repeated calls within the same window are fast.
    /// </summary>
    public async Task<AllCandidatesDetailedReportResponse> GetAllCandidatesDetailedReportAsync(
        int page,
        int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Invalid pagination parameters");

        // Fetch the paged candidate slice + all supporting data in parallel
        var candidatePageTask = _candidates.GetPageAsync(page, pageSize);
        var allInterviewsTask = GetInterviewsCachedAsync();
        var requirementPageTask = _requirements.GetPageAsync(1, int.MaxValue);

        await Task.WhenAll(candidatePageTask, allInterviewsTask, requirementPageTask);

        var candidatePage = candidatePageTask.Result;
        var allInterviews = allInterviewsTask.Result;
        var requirements = requirementPageTask.Result.Data;

        // Pre-group interviews by candidate for O(1) lookup
        var interviewsByCandidateId = allInterviews
            .GroupBy(i => i.CandidateId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var details = candidatePage.Data.Select(candidate =>
        {
            var interviews = interviewsByCandidateId
                .GetValueOrDefault(candidate.Id, new List<InterviewDto>());

            var requirementSummaries = requirements
                .Select(r => new RequirementSummaryDto
                {
                    RequirementId = r.Id,
                    Project = r.Project,
                    Matched = candidate.SkillSet != null &&
                                    candidate.SkillSet.Contains(r.SkillsNeeded ?? ""),
                    Interviewed = interviews.Any(i => i.Project == r.Project),
                    Selected = interviews.Any(i =>
                                        i.Project == r.Project &&
                                        i.FinalOutcome == "Selected")
                })
                .ToList();

            return new CandidateDetailedReportResponse
            {
                Candidate = candidate,
                Interviews = interviews,
                Requirements = requirementSummaries,
                BlockedBySixMonthRule = interviews
                    .Any(i => i.InterviewDate >= sixMonthsAgo)
            };
        }).ToList();

        return new AllCandidatesDetailedReportResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = candidatePage.TotalCount,
            TotalPages = candidatePage.TotalPages,
            Candidates = details
        };
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  EXISTING METHODS (unchanged)
    // ════════════════════════════════════════════════════════════════════════════

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

        await Task.WhenAll(candidatesTask, interviewsTask);

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

        var available = candidates.Count(x => x.AvailabilityDate <= DateTime.UtcNow);
        var percent = candidates.Count == 0
            ? 0
            : (double)available / candidates.Count * 100;

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

    public async Task<CandidateReportPagedResponse> GetCandidateReportPagedAsync(
        int page,
        int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Invalid pagination parameters");

        var candidatePage = await _candidates.GetPageAsync(page, pageSize);
        var interviews = await _interviews.GetAllAsync();
        var candidates = candidatePage.Data;
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

        var available = candidates.Count(c => c.AvailabilityDate <= DateTime.UtcNow);
        var percent = candidates.Count == 0
            ? 0
            : (double)available / candidates.Count * 100;

        return new CandidateReportPagedResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCandidates = candidatePage.TotalCount,
            TotalPages = candidatePage.TotalPages,
            SkillsDistribution = skills,
            ProficiencyDistribution = proficiency,
            AvailableCandidates = available,
            AvailabilityPercentage = Math.Round(percent, 2),
            BlockedCandidatesLast6Months = blocked
        };
    }

    public async Task<InterviewValidationReportResponse> GetInterviewValidationReportAsync()
    {
        var interviews = await _interviews.GetAllAsync();
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var blocked =
            interviews
                .Where(i => i.InterviewDate >= sixMonthsAgo)
                .GroupBy(i => new { i.CandidateId, i.Project })
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.InterviewDate).First();
                    var eligible = latest.InterviewDate.AddMonths(6);
                    var days = (eligible - DateTime.UtcNow).Days;

                    return new BlockedCandidateInfo
                    {
                        CandidateId = g.Key.CandidateId,
                        Project = g.Key.Project,
                        LastInterviewDate = latest.InterviewDate,
                        DaysUntilEligible = Math.Max(days, 0)
                    };
                })
                .ToList();

        return new InterviewValidationReportResponse
        {
            BlockedCandidates = blocked
        };
    }

    public async Task<RequirementFulfillmentReportResponse> GetRequirementFulfillmentReportAsync()
    {
        var paged = await GetRequirementFulfillmentPagedAsync(1, int.MaxValue);

        return new RequirementFulfillmentReportResponse
        {
            Requirements = paged.Requirements
        };
    }

    public async Task<OutcomeReportResponse> GetOutcomeReportAsync()
    {
        var interviews = await _interviews.GetAllAsync();

        var breakdown =
            interviews
                .GroupBy(i =>
                    string.IsNullOrWhiteSpace(i.FinalOutcome)
                        ? "Unknown"
                        : i.FinalOutcome)
                .ToDictionary(x => x.Key, x => x.Count());

        return new OutcomeReportResponse
        {
            TotalInterviews = interviews.Count,
            OutcomeBreakdown = breakdown
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

        // Warm-up
        await _requirements.MatchAsync(requirementId);

        for (int i = 0; i < requestCount; i++)
        {
            var sw = Stopwatch.StartNew();
            await _requirements.MatchAsync(requirementId);
            sw.Stop();
            latencies.Add(sw.Elapsed.TotalMilliseconds);
        }

        latencies.Sort();

        var p95Index = Math.Clamp((int)Math.Ceiling(0.95 * latencies.Count) - 1, 0, latencies.Count - 1);

        return new PerformanceReportResponse
        {
            TotalRequests = requestCount,
            AverageLatencyMs = Math.Round(latencies.Average(), 2),
            MinLatencyMs = Math.Round(latencies.First(), 2),
            MaxLatencyMs = Math.Round(latencies.Last(), 2),
            P95LatencyMs = Math.Round(latencies[p95Index], 2)
        };
    }

    public async Task<CandidateDetailedReportResponse?> GetCandidateDetailedReportAsync(int id)
    {
        var candidate = await _candidates.GetByIdAsync(id);

        if (candidate == null)
            return null;

        var interviews = await _interviews.GetByCandidateAsync(id);
        var requirementPage = await _requirements.GetPageAsync(1, int.MaxValue);
        var requirements = requirementPage.Data;

        return new CandidateDetailedReportResponse
        {
            Candidate = candidate,
            Interviews = interviews,
            Requirements = requirements
                .Select(r => new RequirementSummaryDto
                {
                    RequirementId = r.Id,
                    Project = r.Project,
                    Matched = candidate.SkillSet != null &&
                                    candidate.SkillSet.Contains(r.SkillsNeeded ?? ""),
                    Interviewed = interviews.Any(i => i.Project == r.Project),
                    Selected = interviews.Any(i =>
                                        i.Project == r.Project &&
                                        i.FinalOutcome == "Selected")
                })
                .ToList(),
            BlockedBySixMonthRule = interviews
                .Any(i => i.InterviewDate >= DateTime.UtcNow.AddMonths(-6))
        };
    }

    public async Task<RequirementFulfillmentPagedResponse> GetRequirementFulfillmentPagedAsync(
        int page,
        int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Invalid pagination parameters");

        var requirementPage = await _requirements.GetPageAsync(page, pageSize);
        var candidates = await GetCandidatesCachedAsync();
        var interviews = await GetInterviewsCachedAsync();

        var processedCandidates = candidates.Select(c => new
        {
            Candidate = c,
            Skills = (c.SkillSet ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
        }).ToList();

        var interviewStats = interviews
            .GroupBy(i => i.Project)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Interviewed = g.Select(x => x.CandidateId).Distinct().Count(),
                    Selected = g.Where(x => x.FinalOutcome == "Selected")
                                   .Select(x => x.CandidateId).Distinct().Count()
                });

        var result = new List<RequirementFulfillmentInfo>();

        foreach (var requirement in requirementPage.Data)
        {
            var requirementSkills =
                (requirement.SkillsNeeded ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

            int matchedCandidates = processedCandidates.Count(pc =>
            {
                var c = pc.Candidate;

                if (!requirementSkills.All(pc.Skills.Contains))
                    return false;

                if (c.ExperienceMonths < requirement.MinExperienceMonths ||
                    c.ExperienceMonths > requirement.MaxExperienceMonths)
                    return false;

                if (!string.IsNullOrWhiteSpace(requirement.RequiredPrimarySkillLevel) &&
                    c.PrimarySkillLevel != requirement.RequiredPrimarySkillLevel)
                    return false;

                return true;
            });

            interviewStats.TryGetValue(requirement.Project, out var stat);

            int interviewedCandidates = stat?.Interviewed ?? 0;
            int selectedCandidates = stat?.Selected ?? 0;

            double fulfillmentPercentage =
                matchedCandidates == 0
                    ? 0
                    : (double)selectedCandidates / matchedCandidates * 100;

            result.Add(new RequirementFulfillmentInfo
            {
                RequirementId = requirement.Id,
                Project = requirement.Project,
                MatchedCandidates = matchedCandidates,
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

    // ════════════════════════════════════════════════════════════════════════════
    //  CACHE HELPERS
    // ════════════════════════════════════════════════════════════════════════════

    private async Task<List<CandidateDto>> GetCandidatesCachedAsync()
    {
        try
        {
            const string key = "candidates:all";
            var cached = await _cache.GetStringAsync(key);

            if (cached != null)
                return JsonSerializer.Deserialize<List<CandidateDto>>(cached, CacheJsonOptions)!;

            var data = await _candidates.GetAllAsync();

            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(data, CacheJsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return data;
        }
        catch
        {
            return await _candidates.GetAllAsync();
        }
    }

    private async Task<List<InterviewDto>> GetInterviewsCachedAsync()
    {
        try
        {
            const string key = "interviews:all";
            var cached = await _cache.GetStringAsync(key);

            if (cached != null)
                return JsonSerializer.Deserialize<List<InterviewDto>>(cached, CacheJsonOptions)!;

            var data = await _interviews.GetAllAsync();

            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(data, CacheJsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return data;
        }
        catch
        {
            return await _interviews.GetAllAsync();
        }
    }
}