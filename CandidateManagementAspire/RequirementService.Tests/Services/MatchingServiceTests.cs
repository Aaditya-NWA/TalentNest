using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RequirementService.Contracts.Clients;
using RequirementService.Controllers;
using RequirementService.Data;
using RequirementService.DTOs.External;
using RequirementService.Models;
using RequirementService.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace RequirementService.Tests.Services
{
    [ExcludeFromCodeCoverage]
    public class MatchingServiceTests
    {
        private RequirementDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<RequirementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RequirementDbContext(options);
        }

        // ======================================================
        // MATCHCANDIDATESASYNC
        // ======================================================

        [Test]
        public void MatchCandidates_ShouldThrow_WhenIdNegative()
        {
            var service = new MatchingService(
                CreateDb(),
                Mock.Of<ICandidateClient>());

            Action act = () => service.MatchCandidatesAsync(-1).GetAwaiter().GetResult();

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void MatchCandidates_ShouldThrow_WhenRequirementNotFound()
        {
            var service = new MatchingService(
                CreateDb(),
                Mock.Of<ICandidateClient>());

            Action act = () => service.MatchCandidatesAsync(1).GetAwaiter().GetResult();

            act.Should().Throw<Exception>()
                .WithMessage("Requirement not found");
        }

        [Test]
        public async Task MatchCandidates_ShouldReturnMappedCandidates()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 1,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(10),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1, 10, "C#",
                    null,
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    200))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                        new CandidateDto
                        {
                            Id = 10,
                            Name = "Test",
                            SkillSet = "C#",
                            ExperienceMonths = 5,
                            AvailabilityDate = DateTime.UtcNow,
                            PrimarySkillLevel = "P2"
                        }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.MatchCandidatesAsync(1);

            result.Should().HaveCount(1);
            result.First().CandidateId.Should().Be(10);
        }

        // ======================================================
        // GETRANKEDMATCHESASYNC
        // ======================================================

        [Test]
        public void GetRankedMatches_ShouldThrow_WhenIdInvalid()
        {
            var service = new MatchingService(
                CreateDb(),
                Mock.Of<ICandidateClient>());

            Action act = () => service.GetRankedMatchesAsync(0).GetAwaiter().GetResult();

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void GetRankedMatches_ShouldThrow_WhenRequirementMissing()
        {
            var service = new MatchingService(
                CreateDb(),
                Mock.Of<ICandidateClient>());

            Action act = () => service.GetRankedMatchesAsync(1).GetAwaiter().GetResult();

            act.Should().Throw<KeyNotFoundException>();
        }

        [Test]
        public async Task GetRankedMatches_ShouldCalculateScoreAndOrder()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#,SQL",
                MinExperienceMonths = 5,
                MaxExperienceMonths = 20,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(30),
                RequiredPrimarySkillLevel = "P2"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    5, 20,
                    "C#,SQL",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P2",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                        new CandidateDto
                        {
                            Id = 1,
                            SkillSet = "C#,SQL",
                            ExperienceMonths = 10,
                            AvailabilityDate = DateTime.UtcNow,
                            PrimarySkillLevel = "P3"
                        },
                        new CandidateDto
                        {
                            Id = 2,
                            SkillSet = "C#",
                            ExperienceMonths = 5,
                            AvailabilityDate = DateTime.UtcNow.AddDays(5),
                            PrimarySkillLevel = "P2"
                        }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(2);
            result.First().Score.Should().BeGreaterThan(result.Last().Score);
        }

        // ======================================================
        // EDGE CASES
        // ======================================================

        [Test]
        public async Task GetRankedMatches_ShouldHandle_InvalidPrimarySkillLevel()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "",
                MinExperienceMonths = 0,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = ""
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    0, 10,
                    "",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                        new CandidateDto
                        {
                            Id = 1,
                            SkillSet = "",
                            ExperienceMonths = 0,
                            AvailabilityDate = DateTime.UtcNow,
                            PrimarySkillLevel = "X"
                        }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetRankedMatches_ShouldHandle_ZeroRequiredSkills()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "", // ZERO skills
                MinExperienceMonths = 5,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();
            mockClient.Setup(x => x.SearchCandidatesAsync(
                    5, 10, "",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }

        [Test]
        public async Task GetRankedMatches_ShouldHandle_MinExperienceZero()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 0, // branch
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();
            mockClient.Setup(x => x.SearchCandidatesAsync(
                    0, 10, "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 0,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }

        [Test]
        public async Task GetRankedMatches_ShouldHandle_AvailabilityFail()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 1,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();
            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1, 10, "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow.AddDays(10), // FAIL branch
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }

        [Test]
        public async Task GetRankedMatches_ShouldHandle_InvalidLevelFormat()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 1,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "X" // invalid
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();
            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1, 10, "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "X",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "Z"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetRankedMatches_ShouldCover_ExperienceLessThanMin()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 10,
                MaxExperienceMonths = 20,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    10, 20,
                    "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5, // LESS THAN MIN
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetRankedMatches_ShouldCover_ProficiencyLessThanRequired()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 1,
                MaxExperienceMonths = 20,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P3"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1, 20,
                    "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P3",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1" // LESS THAN REQUIRED
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetRankedMatches_ShouldCover_WhenNoRequiredSkills()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "", // 🔥 EMPTY SKILLS
                MinExperienceMonths = 1,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1,
                    10,
                    "",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow,
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetRankedMatches_ShouldCover_AvailabilityAfterStart()
        {
            var db = CreateDb();

            db.Requirements.Add(new Requirement
            {
                Id = 1,
                SkillsNeeded = "C#",
                MinExperienceMonths = 1,
                MaxExperienceMonths = 10,
                AvailabilityStart = DateTime.UtcNow,
                AvailabilityEnd = DateTime.UtcNow.AddDays(5),
                RequiredPrimarySkillLevel = "P1"
            });

            db.SaveChanges();

            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1,
                    10,
                    "C#",
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    "P1",
                    1,
                    1000))
                .ReturnsAsync(new PaginatedCandidateResponse
                {
                    Data = new List<CandidateDto>
                    {
                new CandidateDto
                {
                    Id = 1,
                    SkillSet = "C#",
                    ExperienceMonths = 5,
                    AvailabilityDate = DateTime.UtcNow.AddDays(10), // AFTER START
                    PrimarySkillLevel = "P1"
                }
                    }
                });

            var service = new MatchingService(db, mockClient.Object);

            var result = await service.GetRankedMatchesAsync(1);

            result.Should().HaveCount(1);
        }
        [Test]
        public async Task GetByExperience_ShouldReturnBadRequest_WhenMinGreaterThanMax()
        {
            var controller = new RequirementFilterController(
                Mock.Of<ICandidateClient>());

            var result = await controller.GetByExperience(10, 5);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Test]
        public async Task GetByAvailability_ShouldReturnBadRequest_WhenStartAfterEnd()
        {
            var controller = new RequirementFilterController(
                Mock.Of<ICandidateClient>());

            var result = await controller.GetByAvailability(
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnBadRequest_WhenWhitespace()
        {
            var controller = new RequirementFilterController(
                Mock.Of<ICandidateClient>());

            var result = await controller.GetByPrimarySkillLevel("   ");

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Test]
        public async Task GetByExperience_ShouldReturnOk_WithResult()
        {
            var mockClient = new Mock<ICandidateClient>();

            var expected = new PaginatedCandidateResponse
            {
                Data = new List<CandidateDto>()
            };

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    1, 5,
                    null,
                    null,
                    null,
                    null,
                    1,
                    50))
                .ReturnsAsync(expected);

            var controller = new RequirementFilterController(mockClient.Object);

            var result = await controller.GetByExperience(1, 5);

            var ok = result as OkObjectResult;

            ok.Should().NotBeNull();
            ok!.Value.Should().BeSameAs(expected);
        }
        [Test]
        public async Task GetByAvailability_ShouldReturnOk_WithResult()
        {
            var mockClient = new Mock<ICandidateClient>();

            var expected = new PaginatedCandidateResponse
            {
                Data = new List<CandidateDto>()
            };

            var start = DateTime.UtcNow;
            var end = start.AddDays(5);

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    0,
                    int.MaxValue,
                    null,
                    start,
                    end,
                    null,
                    1,
                    50))
                .ReturnsAsync(expected);

            var controller = new RequirementFilterController(mockClient.Object);

            var result = await controller.GetByAvailability(start, end);

            var ok = result as OkObjectResult;

            ok.Should().NotBeNull();
            ok!.Value.Should().BeSameAs(expected);
        }
        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnOk_WithResult()
        {
            var mockClient = new Mock<ICandidateClient>();

            var expected = new PaginatedCandidateResponse
            {
                Data = new List<CandidateDto>()
            };

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    0,
                    int.MaxValue,
                    null,
                    null,
                    null,
                    "P1",
                    1,
                    50))
                .ReturnsAsync(expected);

            var controller = new RequirementFilterController(mockClient.Object);

            var result = await controller.GetByPrimarySkillLevel("P1");

            var ok = result as OkObjectResult;

            ok.Should().NotBeNull();
            ok!.Value.Should().BeSameAs(expected);
        }
        [Test]
        public async Task GetByExperience_ShouldThrow_WhenClientThrows()
        {
            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new Exception("Client failure"));

            var controller = new RequirementFilterController(mockClient.Object);

            Func<Task> act = async () => await controller.GetByExperience(1, 5);

            await act.Should().ThrowAsync<Exception>();
        }
        [Test]
        public async Task GetByAvailability_ShouldThrow_WhenClientThrows()
        {
            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new Exception("Client failure"));

            var controller = new RequirementFilterController(mockClient.Object);

            Func<Task> act = async () =>
                await controller.GetByAvailability(DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

            await act.Should().ThrowAsync<Exception>();
        }
        [Test]
        public async Task GetByPrimarySkillLevel_ShouldThrow_WhenClientThrows()
        {
            var mockClient = new Mock<ICandidateClient>();

            mockClient.Setup(x => x.SearchCandidatesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new Exception("Client failure"));

            var controller = new RequirementFilterController(mockClient.Object);

            Func<Task> act = async () =>
                await controller.GetByPrimarySkillLevel("P1");

            await act.Should().ThrowAsync<Exception>();
        }                   
    }

}