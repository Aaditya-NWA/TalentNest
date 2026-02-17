using CandidateService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Diagnostics.CodeAnalysis;


namespace CandidateService.Tests.Infrastructure;

[ExcludeFromCodeCoverage]
internal static class DbContextFactory
{
    public static CandidateDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new CandidateDbContext(options);
    }
}
