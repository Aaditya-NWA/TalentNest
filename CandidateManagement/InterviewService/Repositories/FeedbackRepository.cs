using InterviewService.Contracts.Repositories;
using InterviewService.Data;
using InterviewService.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Repositories;

[ExcludeFromCodeCoverage]
public class FeedbackRepository : IFeedbackRepository
{
    private readonly InterviewDbContext _context;

    public FeedbackRepository(InterviewDbContext context)
    {
        _context = context;
    }

    // ============ CREATE ============
    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    // ============ READ ============
    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks
            .Include(f => f.Interview)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Feedback>> GetByInterviewIdAsync(int interviewId)
    {
        return await _context.Feedbacks
            .Where(f => f.InterviewId == interviewId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    // ✅ IMPLEMENTATION: Get all feedbacks
    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.Interview)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    // ✅ IMPLEMENTATION: Get all feedbacks with date filters
    public async Task<IEnumerable<Feedback>> GetAllAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Feedbacks
            .Include(f => f.Interview)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(f => f.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(f => f.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    // ✅ IMPLEMENTATION: Get feedbacks by creator
    public async Task<IEnumerable<Feedback>> GetByCreatedByAsync(string createdBy)
    {
        return await _context.Feedbacks
            .Include(f => f.Interview)
            .Where(f => f.CreatedBy.ToLower() == createdBy.ToLower())
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    // ============ UPDATE ============
    public async Task<Feedback?> UpdateAsync(int id, Feedback feedback)
    {
        var existing = await _context.Feedbacks.FindAsync(id);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(feedback);
        await _context.SaveChangesAsync();
        return existing;
    }

    // ============ DELETE ============
    public async Task<bool> DeleteAsync(int id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null)
            return false;

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============ UTILITY ============
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Feedbacks.AnyAsync(f => f.Id == id);
    }

    public async Task<int> GetFeedbackCountByInterviewAsync(int interviewId)
    {
        return await _context.Feedbacks
            .CountAsync(f => f.InterviewId == interviewId);
    }

    public async Task<double> GetAverageScoreByInterviewAsync(int interviewId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(f => f.InterviewId == interviewId)
            .ToListAsync();

        if (!feedbacks.Any())
            return 0;

        return feedbacks.Average(f => (f.TechnicalScore + f.CommunicationScore) / 2.0);
    }
}