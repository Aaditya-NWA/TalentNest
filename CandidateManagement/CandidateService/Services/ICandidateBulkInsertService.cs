using CandidateService.Models;

namespace CandidateService.Services
{
    public interface ICandidateBulkInsertService
    {
        Task<bool> InsertSingleAsync(Candidate candidate);

        Task<(int inserted, int skipped, long timeTakenMs)>
            BulkInsertAsync(List<Candidate> candidates);

        Task<HashSet<string>> GetExistingKeysAsync(IEnumerable<Candidate> candidates);
    }
}
