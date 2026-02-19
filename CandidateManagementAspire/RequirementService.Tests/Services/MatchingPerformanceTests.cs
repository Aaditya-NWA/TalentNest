using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RequirementService.Contracts.Clients;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.Models;
using RequirementService.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Tests;

[TestFixture]

[ExcludeFromCodeCoverage]
public class MatchingPerformanceTests
{

    private MatchingService _matchingService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<RequirementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RequirementDbContext(options);

        // Seed Requirement
        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#,SQL",
            MinExperienceMonths = 24,
            AvailabilityStart = DateTime.UtcNow.AddDays(30),
            RequiredPrimarySkillLevel = "L2"
        });

        context.SaveChanges();

        // Seed 10k Candidates
        var candidates = new List<CandidateDto>();

        for (int i = 1; i <= 10000; i++)
        {
            candidates.Add(new CandidateDto
            {
                Id = i,
                Name = $"Candidate {i}",
                SkillSet = "C#,SQL,Microservices",
                ExperienceMonths = 36,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "L3"
            });
        }

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
                  .ReturnsAsync(candidates);

        _matchingService = new MatchingService(context, mockClient.Object);
    }

    [Test]
    public async Task Matching_P95_Should_Be_Less_Than_200ms()
    {
        var latencies = new List<long>();

        for (int i = 0; i < 200; i++)
        {
            var sw = Stopwatch.StartNew();

            await _matchingService.GetRankedMatchesAsync(1);

            sw.Stop();
            latencies.Add(sw.ElapsedMilliseconds);
        }

        latencies.Sort();

        int p95Index = (int)(latencies.Count * 0.95);
        long p95 = latencies[p95Index];

        TestContext.WriteLine($"P95 Latency: {p95} ms");

        Assert.LessOrEqual(p95, 200);
    }
}
