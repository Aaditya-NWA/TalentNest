using InterviewService.Models;
using InterviewService.Models.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace InterviewService.Specifications;

[ExcludeFromCodeCoverage]
public static class InterviewSpecifications
{
    public static Expression<Func<Interview, bool>> ByCandidateId(int candidateId)
    {
        return i => i.CandidateId == candidateId;
    }

    public static Expression<Func<Interview, bool>> ByRequirementId(int requirementId)
    {
        return i => i.RequirementId == requirementId;
    }

    public static Expression<Func<Interview, bool>> ByDateRange(DateTime? fromDate, DateTime? toDate)
    {
        return i =>
            (!fromDate.HasValue || i.InterviewDate >= fromDate.Value) &&
            (!toDate.HasValue || i.InterviewDate <= toDate.Value);
    }

    public static Expression<Func<Interview, bool>> ByProject(string project)
    {
        return i => i.Project == project;
    }

    public static Expression<Func<Interview, bool>> ByOutcome(InterviewOutcome outcome)
    {
        return i => i.FinalOutcome == outcome;
    }

    public static Expression<Func<Interview, bool>> ByLevel(InterviewLevel level)
    {
        return i => i.Level == level;
    }

    public static Expression<Func<Interview, bool>> HasFeedback()
    {
        return i => i.Feedbacks != null && i.Feedbacks.Any();
    }

    public static Expression<Func<Interview, bool>> ScheduledInLastSixMonths(int candidateId, string project, DateTime currentDate)
    {
        var sixMonthsAgo = currentDate.AddMonths(-6);
        return i => i.CandidateId == candidateId &&
                   i.Project == project &&
                   i.InterviewDate >= sixMonthsAgo &&
                   i.InterviewDate < currentDate;
    }

    public static Func<IQueryable<Interview>, IOrderedQueryable<Interview>> OrderByLatestFirst()
    {
        return q => q.OrderByDescending(i => i.InterviewDate);
    }

    public static Func<IQueryable<Interview>, IOrderedQueryable<Interview>> OrderByOldestFirst()
    {
        return q => q.OrderBy(i => i.InterviewDate);
    }
}