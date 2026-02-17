using InterviewService.Contracts.Repositories;
using InterviewService.Data;
using InterviewService.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Repositories;

[ExcludeFromCodeCoverage]
public class InterviewRepository : IInterviewRepository
{
    private readonly InterviewDbContext _context;

    public InterviewRepository(InterviewDbContext context)
    {
        _context = context;
    }

    public async Task<Interview> CreateAsync(Interview interview)
    {
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();
        return interview;
    }

    public async Task<Interview?> GetByIdAsync(int id)
    {
        return await _context.Interviews
            .Include(i => i.Feedbacks)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Interview>> GetByCandidateIdAsync(int candidateId)
    {
        return await _context.Interviews
            .Include(i => i.Feedbacks)
            .Where(i => i.CandidateId == candidateId)
            .OrderByDescending(i => i.InterviewDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Interviews
            .Include(i => i.Feedbacks)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(i => i.InterviewDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.InterviewDate <= toDate.Value);

        return await query
            .OrderByDescending(i => i.InterviewDate)
            .ToListAsync();
    }

    public async Task<Interview?> UpdateAsync(int id, Interview interview)
    {
        var existing = await _context.Interviews.FindAsync(id);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(interview);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var interview = await _context.Interviews.FindAsync(id);
        if (interview == null)
            return false;

        _context.Interviews.Remove(interview);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Interviews.AnyAsync(i => i.Id == id);
    }

    public async Task<(bool IsValid, string ErrorMessage)> HasInterviewedInLastSixMonthsAsync(
    int candidateId,
    string project,
    DateTime interviewDate)
    {
        var sixMonthsAgo = interviewDate.AddMonths(-6);
        var requestedDate = interviewDate.Date;

        var existingInterviews = await _context.Interviews
            .Where(i => i.CandidateId == candidateId &&
                       i.Project.ToLower() == project.ToLower())
            .Select(i => new
            {
                i.InterviewDate,
                IsWithinSixMonths = i.InterviewDate >= sixMonthsAgo && i.InterviewDate < interviewDate,
                IsSameDate = i.InterviewDate.Date == requestedDate
            })
            .ToListAsync();

        // Check for same date first
        var sameDateInterview = existingInterviews.FirstOrDefault(i => i.IsSameDate);
        if (sameDateInterview != null)
        {
            return (false,
                $"Candidate {candidateId} already has an interview scheduled for project '{project}' " +
                $"on {sameDateInterview.InterviewDate:yyyy-MM-dd}. Please choose a different date.");
        }

        // Check for 6-month rule
        var recentInterview = existingInterviews.FirstOrDefault(i => i.IsWithinSixMonths);
        if (recentInterview != null)
        {
            return (false,
                $"Candidate {candidateId} has already appeared for project '{project}' " +
                $"on {recentInterview.InterviewDate:yyyy-MM-dd}, which is within the last 6 months.");
        }

        return (true, "Interview can be scheduled.");
    }

    public async Task<IEnumerable<Interview>> GetInterviewsWithFeedbacksAsync(int candidateId)
    {
        return await _context.Interviews
            .Include(i => i.Feedbacks)
            .Where(i => i.CandidateId == candidateId)
            .OrderByDescending(i => i.InterviewDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByRequirementIdAsync(int requirementId)
    {
        return await _context.Interviews
            .Include(i => i.Feedbacks)
            .Where(i => i.RequirementId == requirementId)
            .OrderByDescending(i => i.InterviewDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetAllAsync()
    {
        return await _context.Interviews
            .Include(i => i.Feedbacks)
            .OrderByDescending(i => i.InterviewDate)
            .ToListAsync();
    }
}