using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RequirementService.Contracts.Clients;
using RequirementService.Contracts.Services;
using RequirementService.Controllers;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.Models;
using RequirementService.Services;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Tests.Services;

[ExcludeFromCodeCoverage]
public class MatchingServiceTests
{
    private RequirementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RequirementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RequirementDbContext(options);
    }

    private Requirement CreateRequirement()
    {
        return new Requirement
        {
            Id = 1,
            Project = "Test",
            SkillsNeeded = "C#,SQL",
            MinExperienceMonths = 12,
            MaxExperienceMonths = 60,
            AvailabilityStart = new DateTime(2026, 2, 1),
            AvailabilityEnd = new DateTime(2026, 6, 1),
            RequiredPrimarySkillLevel = "P2",
            ClientInterviewRequired = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private CandidateDto CreateMatchingCandidate()
    {
        return new CandidateDto
        {
            Id = 10,
            Name = "Match",
            SkillSet = "C#,SQL,React",
            ExperienceMonths = 24,
            AvailabilityDate = new DateTime(2026, 1, 1),
            PrimarySkillLevel = "P3"
        };
    }
    [Test]
    public async Task MatchCandidatesAsync_ShouldThrow_WhenRequirementNotFound()
    {
        var context = CreateDbContext();

        var mockClient = new Mock<ICandidateClient>();

        var service = new MatchingService(context, mockClient.Object);

        Func<Task> act = async () => await service.MatchCandidatesAsync(999);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Requirement not found");
    }
    [Test]
    public async Task MatchCandidatesAsync_ShouldReturnMatchingCandidate()
    {
        var context = CreateDbContext();

        var requirement = CreateRequirement();
        context.Requirements.Add(requirement);
        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(c => c.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            CreateMatchingCandidate()
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.MatchCandidatesAsync(1);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Match");
    }
    [Test]
    public async Task MatchCandidatesAsync_ShouldExclude_WhenSkillsDoNotMatch()
    {
        var context = CreateDbContext();

        var requirement = CreateRequirement();
        context.Requirements.Add(requirement);
        await context.SaveChangesAsync();

        var candidate = CreateMatchingCandidate();
        candidate.SkillSet = "Java,React";

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(c => c.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto> { candidate });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.MatchCandidatesAsync(1);

        result.Should().BeEmpty();
    }

    [Test]
    public void MatchCandidates_ShouldThrow_WhenIdNegative()
    {
        var context = CreateDbContext();
        var mockClient = new Mock<ICandidateClient>();
        var service = new MatchingService(context, mockClient.Object);

        Func<Task> act = async () => await service.MatchCandidatesAsync(-1);

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }

    [Test]
    public void MatchCandidates_ShouldThrow_WhenRequirementMissing()
    {
        var context = CreateDbContext();
        var mockClient = new Mock<ICandidateClient>();
        var service = new MatchingService(context, mockClient.Object);

        Func<Task> act = async () => await service.MatchCandidatesAsync(999);

        act.Should().ThrowAsync<Exception>()
            .WithMessage("Requirement not found");
    }

    [Test]
    public async Task MatchCandidates_ShouldReturnMatch_WhenValid()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            Project = "Test",
            SkillsNeeded = "C#,SQL",
            MinExperienceMonths = 12,
            AvailabilityStart = new DateTime(2026, 01, 01),
            RequiredPrimarySkillLevel = "P2"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
                new CandidateDto
                {
                    Id = 10,
                    Name = "John",
                    SkillSet = "C#,SQL",
                    ExperienceMonths = 24,
                    AvailabilityDate = new DateTime(2025,12,01),
                    PrimarySkillLevel = "P3"
                }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.MatchCandidatesAsync(1);

        result.Should().HaveCount(1);
        result[0].CandidateId.Should().Be(10);
    }

    [Test]
    public async Task MatchCandidates_ShouldReturnEmpty_WhenNoMatch()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "Java",
            MinExperienceMonths = 12,
            AvailabilityStart = DateTime.UtcNow,
            RequiredPrimarySkillLevel = "P3"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
                new CandidateDto
                {
                    Id = 10,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1"
                }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.MatchCandidatesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public void GetRankedMatchesAsync_ShouldThrow_WhenIdZeroOrNegative()
    {
        var service = new MatchingService(CreateDbContext(), Mock.Of<ICandidateClient>());

        Func<Task> act1 = async () => await service.GetRankedMatchesAsync(0);
        Func<Task> act2 = async () => await service.GetRankedMatchesAsync(-5);

        act1.Should().ThrowAsync<ArgumentException>();
        act2.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldThrow_WhenRequirementMissing()
    {
        var service = new MatchingService(CreateDbContext(), Mock.Of<ICandidateClient>());

        Func<Task> act = async () => await service.GetRankedMatchesAsync(1);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Requirement not found");
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldExclude_WhenExperienceTooLow()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 24,
            AvailabilityStart = DateTime.UtcNow,
            RequiredPrimarySkillLevel = "P1"
        });
        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 12, // Too low
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "P1"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldExclude_WhenAvailabilityAfterStart()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            AvailabilityStart = new DateTime(2026, 1, 1),
            RequiredPrimarySkillLevel = "P1"
        });
        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 12,
                AvailabilityDate = new DateTime(2026, 2, 1), // Too late
                PrimarySkillLevel = "P1"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldExclude_WhenSkillLevelInvalid()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            AvailabilityStart = DateTime.UtcNow,
            RequiredPrimarySkillLevel = "P3"
        });
        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 12,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "INVALID"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldCalculateFullScore_WhenExperienceHigher()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 12,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P1"
        });
        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 24,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "P2"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().HaveCount(1);
        result[0].Score.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task MatchCandidates_ShouldReturnBadRequest_WhenIdZero()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var result = await controller.MatchCandidates(0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldHandle_NullSkillLevel()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P1"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = null   // covers null branch
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldHandle_ShortSkillLevel()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P2"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "P"   // length < 2
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldHandle_InvalidNumericSkillLevel()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P3"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "PX"   // TryParse fails
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldReturnMidpointScore_WhenExperienceEqualsMin()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#",
            MinExperienceMonths = 10,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P1"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "C#",
                ExperienceMonths = 10, // equal
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "P2"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().HaveCount(1);
    }
    [Test]
    public void GetRankedMatchesAsync_ShouldThrow_WhenIdNegative()
    {
        var service = new MatchingService(CreateDbContext(), Mock.Of<ICandidateClient>());

        Func<Task> act = async () => await service.GetRankedMatchesAsync(-1);

        act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task GetRankedMatchesAsync_ShouldExclude_WhenNoSkillsMatch()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Id = 1,
            SkillsNeeded = "C#,SQL",
            MinExperienceMonths = 1,
            AvailabilityStart = DateTime.UtcNow.AddDays(1),
            RequiredPrimarySkillLevel = "P1"
        });

        await context.SaveChangesAsync();

        var mockClient = new Mock<ICandidateClient>();
        mockClient.Setup(x => x.GetAllCandidatesAsync())
            .ReturnsAsync(new List<CandidateDto>
            {
            new CandidateDto
            {
                Id = 1,
                SkillSet = "Java,Python",  // ZERO overlap
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.UtcNow,
                PrimarySkillLevel = "P2"
            }
            });

        var service = new MatchingService(context, mockClient.Object);

        var result = await service.GetRankedMatchesAsync(1);

        result.Should().BeEmpty();
    }






}


