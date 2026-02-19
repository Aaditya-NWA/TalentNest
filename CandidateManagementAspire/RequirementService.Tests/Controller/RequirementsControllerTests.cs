using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Mvc; 
using RequirementService.Contracts.Services;
using RequirementService.Controllers;
using RequirementService.Data;
using RequirementService.DTOs.Requests;
using RequirementService.DTOs.Responses;
using RequirementService.Models;
using System.Diagnostics.CodeAnalysis;


namespace RequirementService.Tests.Controllers;

[ExcludeFromCodeCoverage]
public class RequirementsControllerTests
{
    

    private RequirementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RequirementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RequirementDbContext(options);
    }
    [Test]
    public async Task GetAll_ShouldReturnOk()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 12,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            RequiredPrimarySkillLevel = "P1",
            ClientInterviewRequired = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }
    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();

        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.GetById(99);

        result.Should().BeOfType<NotFoundResult>();
    }
    [Test]
    public async Task MatchCandidates_ShouldReturnOk()
    {
        var context = CreateDbContext();

        var mockMatching = new Mock<IMatchingService>();
        mockMatching.Setup(m => m.MatchCandidatesAsync(1))
            .ReturnsAsync(new List<CandidateMatchResponse>());

        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.MatchCandidates(1);

        result.Should().BeOfType<OkObjectResult>();
    }
    [Test]
    public async Task MatchCandidates_ShouldReturnNotFound_WhenExceptionThrown()
    {
        var mockMatching = new Mock<IMatchingService>();
        mockMatching.Setup(m => m.GetRankedMatchesAsync(1))
            .ThrowsAsync(new KeyNotFoundException("Requirement not found"));

        var controller = new RequirementsController(
            CreateDbContext(),
            mockMatching.Object,
            Mock.Of<IRequirementService>());

        var result = await controller.MatchCandidates(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new RequirementService.DTOs.Requests.CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1",
            ClientInterviewRequired = false
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
    }
    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenExperienceInvalid()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new RequirementService.DTOs.Requests.CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "invalid",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenExists()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 12,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            RequiredPrimarySkillLevel = "P1",
            ClientInterviewRequired = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }
    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();

        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.Delete(999);

        result.Should().BeOfType<NotFoundResult>();
    }
    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenAvailabilityInvalid()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new RequirementService.DTOs.Requests.CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "invalid",
            RequiredPrimarySkillLevel = "P1"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenMinGreaterThanMax()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new RequirementService.DTOs.Requests.CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "60,12",
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenExperiencePartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "12,", // second part empty
            AvailabilityWindow = "2026-01-01,2026-06-01",
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task CreateRequirementAsync_ShouldThrow_WhenAvailabilityPartEmpty()
    {
        var context = CreateDbContext();
        var service = new RequirementService.Services.RequirementService(context);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,", // second part empty
            RequiredPrimarySkillLevel = "P1"
        };

        Func<Task> act = async () => await service.CreateRequirementAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Test]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        var context = CreateDbContext();

        context.Requirements.Add(new Requirement
        {
            Project = "Test",
            SkillsNeeded = "C#",
            MinExperienceMonths = 1,
            MaxExperienceMonths = 12,
            AvailabilityStart = DateTime.UtcNow,
            AvailabilityEnd = DateTime.UtcNow.AddMonths(1),
            RequiredPrimarySkillLevel = "P1",
            ClientInterviewRequired = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceNull()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceFirstPartEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = ",12",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceSecondPartEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "12,",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceFirstNonNumeric()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "abc,12",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceSecondNonNumeric()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "12,abc",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilityFirstPartEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = ",2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilitySecondPartEmpty()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenSecondExperienceNegative()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "12,-1",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenBothExperiencePartsNonNumeric()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "abc,xyz",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenBothDatesInvalid()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "invalid,invalid"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldSucceed_WhenStartDateEqualsEndDate()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-01-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilityWindowIsNull()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);
        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = null
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceRangeIsNull()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);
        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = null,
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenExperienceRangeHasNoComma()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "12", // NO COMMA
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilityWindowHasNoComma()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01" // NO COMMA
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnCreatedAtAction_WithCorrectValues()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirementService = new Mock<IRequirementService>();

        var controller = new RequirementsController(context, mockMatching.Object, mockRequirementService.Object);

        var request = new CreateRequirementRequest
        {
            Project = "FinalTest",
            SkillsNeeded = "C#",
            ExperienceRange = "1,12",
            AvailabilityWindow = "2026-01-01,2026-06-01"
        };

        var result = await controller.Create(request);

        var created = result as CreatedAtActionResult;

        created.Should().NotBeNull();
        created!.ActionName.Should().Be("GetById");

        created.RouteValues.Should().ContainKey("id");
        created.Value.Should().BeOfType<Requirement>();

        var returned = created.Value as Requirement;
        returned!.Project.Should().Be("FinalTest");
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenMaxExperienceNegative()
    {
        var context = CreateDbContext();
        var mockMatching = new Mock<IMatchingService>();
        var mockRequirement = new Mock<IRequirementService>();

        var controller = new RequirementsController(
            context,
            mockMatching.Object,
            mockRequirement.Object);

        var request = new CreateRequirementRequest
        {
            Project = "Test",
            SkillsNeeded = "C#",
            ExperienceRange = "1,-5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void GetById_ShouldThrow_WhenIdNegative()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        Action act = () => controller.GetById(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>();
    }


    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenDeleted()
    {
        var context = CreateDbContext();
        context.Requirements.Add(new Requirement { Id = 1, Project = "Test" });
        await context.SaveChangesAsync();

        var controller = new RequirementsController(
            context,
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var result = await controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Match_ShouldReturnOk_WhenSuccess()
    {
        var mockMatch = new Mock<IMatchingService>();
        mockMatch.Setup(x => x.MatchCandidatesAsync(1))
    .ReturnsAsync(new List<CandidateMatchResponse>());


        var controller = new RequirementsController(
            CreateDbContext(),
            mockMatch.Object,
            Mock.Of<IRequirementService>());

        var result = await controller.MatchCandidates(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Match_ShouldReturnNotFound_WhenExceptionThrown()
    {
        var mockMatch = new Mock<IMatchingService>();
        mockMatch.Setup(x => x.GetRankedMatchesAsync(1))
            .ThrowsAsync(new KeyNotFoundException("Requirement not found"));

        var controller = new RequirementsController(
            CreateDbContext(),
            mockMatch.Object,
            Mock.Of<IRequirementService>());

        var result = await controller.MatchCandidates(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public void Delete_ShouldThrow_WhenIdNegative()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        Action act = () => controller.Delete(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public async Task MatchCandidates_ShouldReturnBadRequest_WhenIdNegative()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var result = await controller.MatchCandidates(-1);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Id cannot be negative or zero");
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenMinGreaterThanOrEqualMax()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "5,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenExperienceNegative()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "-1,5",
            AvailabilityWindow = "2026-01-01,2026-02-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }


    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenAvailabilityNull()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = null
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Test]
    public async Task Controller_Create_ShouldReturnBadRequest_WhenStartAfterEnd()
    {
        var controller = new RequirementsController(
            CreateDbContext(),
            Mock.Of<IMatchingService>(),
            Mock.Of<IRequirementService>());

        var request = new CreateRequirementRequest
        {
            ExperienceRange = "1,5",
            AvailabilityWindow = "2026-02-01,2026-01-01"
        };

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
    





}








