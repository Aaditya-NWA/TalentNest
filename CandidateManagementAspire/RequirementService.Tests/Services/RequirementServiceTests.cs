using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RequirementService.Contracts.Services;
using RequirementService.Controllers;
using RequirementService.Data;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Services;
using System.Diagnostics.CodeAnalysis;
using RequirementService.Models;
using RequirementSvc = RequirementService.Services.RequirementService;
using System;
using System.Threading.Tasks;

namespace RequirementService.Tests.Services;

[ExcludeFromCodeCoverage]
public class RequirementServiceTests
{
    private RequirementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RequirementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RequirementDbContext(options);
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldCreate_WhenValid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#,SQL",
            ExperienceRange = "12,60",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P2",
            ClientInterviewRequired = true
        };

        var result = await service.CreateRequirementAsync(request);

        result.Project.Should().Be("Test");
        result.MinExperienceMonths.Should().Be(12);
        result.MaxExperienceMonths.Should().Be(60);
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenExperienceInvalid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "invalid",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenAvailabilityInvalid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "invalid",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task GetRequirementByIdAsync_ShouldReturn_WhenExists()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var created = await service.CreateRequirementAsync(new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        });

        var result = await service.GetRequirementByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Project.Should().Be("Test");
    }
    [Test]
    public async Task GetRequirementByIdAsync_ShouldReturnNull_WhenMissing()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var result = await service.GetRequirementByIdAsync(999);

        result.Should().BeNull();
    }
    [Test]
    public async Task GetAllRequirementsAsync_ShouldReturnOrdered()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        await service.CreateRequirementAsync(new CreateRequirementRequest
        {
            Project = "First",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        });

        await service.CreateRequirementAsync(new CreateRequirementRequest
        {
            Project = "Second",
            SkillsNeeded = "SQL",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        });

        var results = await service.GetAllRequirementsAsync();

        results.Should().HaveCount(2);
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenMinGreaterThanMax()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "60,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenExperienceNull()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenExperienceNegative()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "-1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenAvailabilityNull()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenStartAfterEnd()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-06-01,2026-01-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenBothExperiencePartsNonNumeric()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "abc,xyz",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenBothDatesInvalid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "invalid,invalid",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilityWindowBothPartsEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirementService.Object);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = ","
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceRangeBothPartsEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirementService.Object);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = ",",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Update_ShouldReturnOk_WhenValid()
    {
        var context = CreateDbContext();

        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var response = new RequirementResponse
        {
            Id = 1,
            Project = "Updated"
        };

        mockRequirementService
            .Setup(x => x.UpdateRequirementAsync(1, It.IsAny<CreateRequirementRequest>()))
            .ReturnsAsync(response);

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Updated",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Update(1, request);

        result.Should().BeOfType<OkObjectResult>();
    }
    [Test]
    public async Task Update_ShouldReturnNotFound_WhenRequirementMissing()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        mockRequirementService
            .Setup(x => x.UpdateRequirementAsync(1, It.IsAny<CreateRequirementRequest>()))
            .ReturnsAsync((RequirementResponse?)null);

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirementService.Object);

        var request = new CreateRequirementRequest();

        var result = await controller.Update(1, request);

        result.Should().BeOfType<NotFoundResult>();
    }
    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenValidationFails()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        mockRequirementService
            .Setup(x => x.UpdateRequirementAsync(1, It.IsAny<CreateRequirementRequest>()))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirementService.Object);

        var request = new CreateRequirementRequest();

        var result = await controller.Update(1, request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateRequirementAsync_ShouldReturnNull_WhenRequirementNotFound()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        var result = await service.UpdateRequirementAsync(999, request);

        result.Should().BeNull();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceRangeEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var requirement = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(requirement.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceFormatInvalid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "abc,xyz",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityInvalid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "invalid"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldUpdate_WhenValid()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);


        var req = context.Requirements.Add(new Requirement
        {
            Project = "Old",
            SkillsNeeded = "OldSkill",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            Project = "New",
            SkillsNeeded = "C#",
            ExperienceRange = "2,10",
            AvailabilityWindow = "2026-01-01,2026-03-01"
        };

        var result = await service.UpdateRequirementAsync(req.Id, request);

        result.Should().NotBeNull();
        result!.Project.Should().Be("New");
        result.MinExperienceMonths.Should().Be(2);
        result.MaxExperienceMonths.Should().Be(10);
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceRangeInvalidFormat()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "15", // ❌ no comma
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceNegative()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "-1,5",   // negative value
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMinGreaterThanMax()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "10,5", // ❌ invalid
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityInvalidFormat()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "invalid"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceRangeNull()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = null,
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityWindowNull()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = null
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenStartDateEqualsEndDate()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-02-01,2026-01-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMinEqualsMax()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "5,5", // equal values
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceRangeHasOnePart()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "5",  // only one part
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMinExperienceNegative()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "-1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceNotInteger()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "abc,xyz",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityFormatWrong()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01" // only one value
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMaxExperienceNegative()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var req = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,-5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(req.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMaxExperienceIsNegative()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,-5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }


    [Test]
    public async Task CreateRequirement_ShouldThrow_WhenExperienceRangeEmpty()
    {
        var service = new RequirementSvc(CreateDbContext()
);


        var request = new CreateRequirementRequest
        {
            ExperienceRange = "",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task CreateRequirement_ShouldThrow_WhenExperienceNegative()
    {
        var service = new RequirementSvc(CreateDbContext());


        var request = new CreateRequirementRequest
        {
            ExperienceRange = "-1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

    }

    [Test]
    public async Task CreateRequirement_ShouldThrow_WhenMinGreaterThanMax()
    {
        var service = new RequirementSvc(CreateDbContext());

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "10,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

    }

    [Test]
    public async Task CreateRequirement_ShouldThrow_WhenAvailabilityInvalid()
    {
        var service = new RequirementSvc(CreateDbContext());


        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "invalid"
        };

        Func<Task> act = async () =>
        await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GetRequirementById_ShouldThrow_WhenIdNegative()
    {
        var service = new RequirementSvc(CreateDbContext());

        Action act = () => service.GetRequirementByIdAsync(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }

    [Test]
    public async Task UpdateRequirement_ShouldThrow_WhenIdNegative()
    {
        var service = new RequirementSvc(CreateDbContext());


        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(-1, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperiencePartsEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);


        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = ",",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
    await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();

    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityPartsEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = ","
        };

        Func<Task> act = async () =>
    await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();

    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenIdNegative_ExactMessage()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
            await service.UpdateRequirementAsync(-10, request);

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldAllow_WhenStartEqualsEnd()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01,2026-01-01"
        };

        var result = await service.UpdateRequirementAsync(entity.Id, request);

        result.Should().NotBeNull();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceFirstPartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = " ,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenExperienceSecondPartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "5, ",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityFirstPartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = " ,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilitySecondPartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementSvc(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-01-01, "
        };

        Func<Task> act = async () =>
       await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMinWhitespace()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = " ,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenMaxWhitespace()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "5, ",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
       await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenBothPartsWhitespace()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = " , ",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        Func<Task> act = async () =>
        await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task UpdateRequirementAsync_ShouldThrow_WhenAvailabilityWhitespace()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var entity = context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 5,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        }).Entity;

        await context.SaveChangesAsync();

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = " , "
        };

        Func<Task> act = async () =>
       await service.UpdateRequirementAsync(entity.Id, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }






}


