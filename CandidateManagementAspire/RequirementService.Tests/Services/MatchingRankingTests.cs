using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.Models;
using RequirementService.Services;
using RequirementService.Contracts.Clients;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public class MatchingRankingTests
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

        var candidates = new List<CandidateDto>
        {
            // Perfect match
            new CandidateDto
            {
                Id = 1,
                Name = "Perfect",
                SkillSet = "C#,SQL",
                ExperienceMonths = 36,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "L3"
            },

            // Partial match (1 skill)
            new CandidateDto
            {
                Id = 2,
                Name = "Partial",
                SkillSet = "C#",
                ExperienceMonths = 30,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "L2"
            },

            // Lower experience
            new CandidateDto
            {
                Id = 3,
                Name = "Low",
                SkillSet = "C#,SQL",
                ExperienceMonths = 24,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "L2"
            }
        };

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
                  .ReturnsAsync(candidates);

        _matchingService = new MatchingService(context, mockClient.Object);
    }

    [Test]
    public async Task RankedCandidates_Should_Be_Ordered_By_Score_Descending()
    {
        var result = await _matchingService.GetRankedMatchesAsync(1);

        Assert.That(result.Count, Is.EqualTo(3));

        // First should be highest score
        Assert.That(result[0].Score, Is.GreaterThanOrEqualTo(result[1].Score));
        Assert.That(result[1].Score, Is.GreaterThanOrEqualTo(result[2].Score));

        // Perfect match should be first
        Assert.That(result[0].CandidateId, Is.EqualTo(1));
    }
}
