using CandidateService.Services;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

public class SqlBulkCopyFactoryTests
{

    [ExcludeFromCodeCoverage]
    private class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; } = "";
        public override string Database => "";
        public override string DataSource => "";
        public override string ServerVersion => "";
        public override ConnectionState State => ConnectionState.Open;

        public override void Open() { }
        public override void Close() { }
        public override void ChangeDatabase(string databaseName) { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null!;
        protected override DbCommand CreateDbCommand() => null!;
    }
}
