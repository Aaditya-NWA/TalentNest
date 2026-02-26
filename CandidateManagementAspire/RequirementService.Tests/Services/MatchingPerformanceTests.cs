using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using RequirementService.Contracts.Clients;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.Models;
using RequirementService.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

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

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#,SQL",
            MinExperienceMonths = 24,
            AvailabilityStart = DateTime.UtcNow.AddDays(30),
            RequiredPrimarySkillLevel = "L2"
        });

        context.SaveChanges();

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

        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((byte[]?)null);
        cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        _matchingService = new MatchingService(context, mockClient.Object, cacheMock.Object);
    }
}