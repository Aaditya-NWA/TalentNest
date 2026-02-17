using System;
using System.Data;
using System.Threading.Tasks;

namespace CandidateService.Services
{
    public interface IBulkCopy : IDisposable
    {
        string DestinationTableName { get; set; }
        int BulkCopyTimeout { get; set; }

        // Simplified column mapping helper used by the service
        void ColumnMappings_Add(string source, string destination);

        Task WriteToServerAsync(DataTable table);
    }
}