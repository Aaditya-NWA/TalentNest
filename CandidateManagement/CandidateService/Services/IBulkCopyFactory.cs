using System.Data.Common;

namespace CandidateService.Services
{
    public interface IBulkCopyFactory
    {
        // Accepts a DbConnection which may be null for non-relational providers (tests)
        IBulkCopy Create(DbConnection? connection);
    }
}