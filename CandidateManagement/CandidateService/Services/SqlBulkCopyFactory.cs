using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CandidateService.Services
{
    [ExcludeFromCodeCoverage]
    internal class SqlBulkCopyWrapper : IBulkCopy
    {
        private readonly SqlBulkCopy _bulkCopy;

        public SqlBulkCopyWrapper(SqlConnection connection)
        {
            _bulkCopy = new SqlBulkCopy(connection);
        }

        public string DestinationTableName
        {
            get => _bulkCopy.DestinationTableName;
            set => _bulkCopy.DestinationTableName = value;
        }

        public int BulkCopyTimeout
        {
            get => _bulkCopy.BulkCopyTimeout;
            set => _bulkCopy.BulkCopyTimeout = value;
        }

        public void ColumnMappings_Add(string source, string destination)
            => _bulkCopy.ColumnMappings.Add(source, destination);

        public Task WriteToServerAsync(DataTable table)
            => _bulkCopy.WriteToServerAsync(table);

        // SqlBulkCopy implements IDisposable via an explicit interface in some providers,
        // so call Dispose through the IDisposable interface to avoid CS1061.
        public void Dispose() => ((IDisposable)_bulkCopy).Dispose();
    }

    public class SqlBulkCopyFactory : IBulkCopyFactory
    {
        [ExcludeFromCodeCoverage]
        public IBulkCopy Create(DbConnection? connection)
        {
            if (connection is SqlConnection sqlConnection)
                return new SqlBulkCopyWrapper(sqlConnection);

            throw new InvalidOperationException("SqlBulkCopyFactory requires a SqlConnection for production bulk copy.");
        }
    }
}