using CandidateService.Controllers;
using CandidateService.Data;
using CandidateService.DTOs;
using CandidateService.Models;
using CandidateService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;



[ExcludeFromCodeCoverage]


public class CandidateControllerTests
{

    
    private CandidateDbContext _context = null!;
    private CandidateController _controller = null!;
    private Mock<ICandidateBulkInsertService> _bulkService = null!;

    [ExcludeFromCodeCoverage]
    private CandidateDbContext CreateDb()
    {

        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CandidateDbContext(options);
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Create_ValidRequest_ReturnsOk()
    {
        var db = CreateDb();
        var service = new Mock<ICandidateBulkInsertService>();
        service.Setup(s => s.InsertSingleAsync(It.IsAny<Candidate>()))
               .ReturnsAsync(true);

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 12,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        };

        var result = await controller.Create(req);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Create_Duplicate_ReturnsConflict()
    {
        var db = CreateDb();

        var service = new Mock<ICandidateBulkInsertService>();
        service.Setup(s => s.InsertSingleAsync(It.IsAny<Candidate>()))
               .ReturnsAsync(false); // simulate duplicate

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 12,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        };

        var result = await controller.Create(req);

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetById_Existing_ReturnsOk()
    {
        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Id = 1,
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 10,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var controller = new CandidateController(db, Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetById(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetById_NotFound_Returns404()
    {
        var controller = new CandidateController(
            CreateDb(),
            Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetById(99);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_ReturnsPagedResult()
    {
        var db = CreateDb();
        db.Candidates.AddRange(
            new Candidate { Name = "A", MailId = "a@test.com", SkillSet = "C#", ExperienceMonths = 1, AvailabilityDate = DateTime.Today, PrimarySkillLevel = "P1" },
            new Candidate { Name = "B", MailId = "b@test.com", SkillSet = "Java", ExperienceMonths = 2, AvailabilityDate = DateTime.Today, PrimarySkillLevel = "P2" }
        );
        db.SaveChanges();

        var controller = new CandidateController(db, Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetAll();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Update_Existing_ReturnsOk()
    {
        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Id = 1,
            Name = "Old",
            MailId = "old@test.com",
            SkillSet = "C#",
            ExperienceMonths = 5,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var service = new Mock<ICandidateBulkInsertService>();
        service.Setup(s => s.GetExistingKeysAsync(It.IsAny<IEnumerable<Candidate>>()))
       .ReturnsAsync(new HashSet<string>());

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "New",
            MailId = "new@test.com",
            SkillSet = "C#",
            ExperienceMonths = 10,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P2"
        };

        var result = await controller.Update(1, req);

        Assert.That(result, Is.InstanceOf<OkResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Id = 1,
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 1,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var controller = new CandidateController(db, Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.Delete(1);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Delete_NotFound_Returns404()
    {
        var controller = new CandidateController(
            CreateDb(),
            Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.Delete(99);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Update_Duplicate_ReturnsConflict()
    {
        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Id = 1,
            Name = "Old",
            MailId = "old@test.com",
            SkillSet = "C#",
            ExperienceMonths = 5,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var service = new Mock<ICandidateBulkInsertService>();
        service.Setup(s => s.GetExistingKeysAsync(It.IsAny<IEnumerable<Candidate>>()))
               .ReturnsAsync(new HashSet<string> { "duplicate-key" });

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "New",
            MailId = "new@test.com",
            SkillSet = "C#",
            ExperienceMonths = 10,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P2"
        };

        var result = await controller.Update(1, req);

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_InvalidPageAndPageSize_DefaultsApplied()
    {
        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 1,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var controller = new CandidateController(db, Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetAll(page: -1, pageSize: 500);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Update_NotFound_Returns404()
    {
        var db = CreateDb(); // no candidate added

        var service = new Mock<ICandidateBulkInsertService>();

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "New",
            MailId = "new@test.com",
            SkillSet = "C#",
            ExperienceMonths = 10,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P2"
        };

        var result = await controller.Update(999, req);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var controller = new CandidateController(
            CreateDb(),
            Mock.Of<ICandidateBulkInsertService>());

        var req = new CreateCandidateRequest(); // invalid

        var result = await controller.Create(req);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task Update_SameKeySameDate_NoConflict()
    {
        var date = DateTime.Today;

        var db = CreateDb();
        db.Candidates.Add(new Candidate
        {
            Id = 1,
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            AvailabilityDate = date,
            ExperienceMonths = 1,
            PrimarySkillLevel = "P1"
        });
        db.SaveChanges();

        var service = new Mock<ICandidateBulkInsertService>();
        service.Setup(s => s.GetExistingKeysAsync(It.IsAny<IEnumerable<Candidate>>()))
               .ReturnsAsync(new HashSet<string> { "dup" });

        var controller = new CandidateController(db, service.Object);

        var req = new CreateCandidateRequest
        {
            Name = "A",
            MailId = "a@test.com",
            SkillSet = "C#",
            AvailabilityDate = date,
            ExperienceMonths = 2,
            PrimarySkillLevel = "P2"
        };

        var result = await controller.Update(1, req);

        Assert.That(result, Is.InstanceOf<OkResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsOk()
    {
        var controller = new CandidateController(
            CreateDb(),
            Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetAll();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_TotalCountExactMultipleOfPageSize()
    {
        var db = CreateDb();

        // EXACT multiple: 100 / 50 = 2.0
        for (int i = 1; i <= 100; i++)
        {
            db.Candidates.Add(new Candidate
            {
                Name = $"C{i}",
                MailId = $"c{i}@test.com",
                SkillSet = "C#",
                ExperienceMonths = i,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P1"
            });
        }
        db.SaveChanges();

        var controller = new CandidateController(db, Mock.Of<ICandidateBulkInsertService>());

        var result = await controller.GetAll(page: 1, pageSize: 50);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    [SetUp]
    [ExcludeFromCodeCoverage]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CandidateDbContext(options);

        _bulkService = new Mock<ICandidateBulkInsertService>();

        _controller = new CandidateController(
            _context,
            _bulkService.Object);
    }
    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_Should_Normalize_Page_When_Invalid()
    {
        var result = await _controller.GetAll(0, 50);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();

        var value = ok!.Value!;

        var pageProperty = value.GetType().GetProperty("page");
        pageProperty.Should().NotBeNull();

        var page = (int)pageProperty!.GetValue(value)!;
        page.Should().Be(1);

    }

    [ExcludeFromCodeCoverage]
    [Test]
    public async Task GetAll_Should_Normalize_PageSize_When_Zero()
    {
        var result = await _controller.GetAll(1, 0);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var value = ok!.Value!;
        var pageSizeProperty = value.GetType().GetProperty("pageSize");
        pageSizeProperty.Should().NotBeNull();

        var pageSize = (int)pageSizeProperty!.GetValue(value)!;
        pageSize.Should().Be(50);

    }

    [Test]
    public void Delete_ShouldThrow_WhenIdIsNegative()
    {
        var context = CreateDb();
        var mockService = new Mock<ICandidateBulkInsertService>();
        var controller = new CandidateController(context, mockService.Object);

        Action act = () => controller.Delete(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenExperienceNegative()
    {
        var context = CreateDb();
        var mockService = new Mock<ICandidateBulkInsertService>();

        var candidate = new Candidate
        {
            Name = "Test",
            MailId = "a@test.com",
            SkillSet = "C#",
            ExperienceMonths = 5,
            AvailabilityDate = DateTime.Today
        };

        context.Candidates.Add(candidate);
        await context.SaveChangesAsync();

        var controller = new CandidateController(context, mockService.Object);

        var request = new CreateCandidateRequest
        {
            ExperienceMonths = -5
        };

        var result = await controller.Update(candidate.Id, request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void GetById_ShouldThrow_WhenIdIsNegative()
    {
        var context = CreateDb();
        var mockService = new Mock<ICandidateBulkInsertService>();
        var controller = new CandidateController(context, mockService.Object);

        Action act = () => controller.GetById(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public void Update_ShouldThrow_WhenIdIsNegative()
    {
        var context = CreateDb();
        var mockService = new Mock<ICandidateBulkInsertService>();
        var controller = new CandidateController(context, mockService.Object);

        var request = new CreateCandidateRequest();

        Action act = () => controller.Update(-1, request).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }










}
