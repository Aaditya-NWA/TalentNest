using CandidateService.Data;
using CandidateService.Models;
using FluentAssertions;
using InterviewService.Contracts.Repositories;
using InterviewService.Models;
using InterviewService.Models.Enums;
using InterviewService.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RequirementService.Data;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Interview = InterviewService.Models.Interview;
using Requirement = RequirementService.Models.Requirement;


namespace InterviewService.Tests.Services;

[TestFixture]
[ExcludeFromCodeCoverage]
public class InterviewValidationServiceTests
{
    private CandidateDbContext _candidateContext = null!;
    private RequirementDbContext _requirementContext = null!;
    private Mock<IInterviewRepository> _repo = null!;
    private InterviewValidationService _service = null!;

    [SetUp]
    public void Setup()
    {
        var candidateOptions = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var requirementOptions = new DbContextOptionsBuilder<RequirementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _candidateContext = new CandidateDbContext(candidateOptions);
        _requirementContext = new RequirementDbContext(requirementOptions);
        _repo = new Mock<IInterviewRepository>();

        _service = new InterviewValidationService(
            _repo.Object,
            _candidateContext,
            _requirementContext);
    }

    // ================= VALIDATION MAIN METHOD =================

    [Test]
    public async Task ValidateInterviewAsync_Should_Fail_When_Candidate_Not_Exists()
    {
        var interview = CreateInterview();

        var result = await _service.ValidateInterviewAsync(interview);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
    }

    [Test]
    public async Task ValidateInterviewAsync_Should_Fail_When_Requirement_Not_Exists()
    {
        _candidateContext.Candidates.Add(new Candidate { Id = 1 });
        await _candidateContext.SaveChangesAsync();

        var interview = CreateInterview();

        var result = await _service.ValidateInterviewAsync(interview);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Requirement ID");
    }

    [Test]
    public async Task ValidateInterviewAsync_Should_Fail_When_6MonthRule_Fails()
    {
        _candidateContext.Candidates.Add(new Candidate { Id = 1 });

        _requirementContext.Requirements.Add(new Requirement
        {
            Id = 1,
            Project = "Test",
            ClientInterviewRequired = true
        });

        await _candidateContext.SaveChangesAsync();
        await _requirementContext.SaveChangesAsync();

        _repo.Setup(x => x.HasInterviewedInLastSixMonthsAsync(
                1, "Test", It.IsAny<DateTime>()))
            .ReturnsAsync((false, "6 month rule failed"));

        var interview = CreateInterview();

        var result = await _service.ValidateInterviewAsync(interview);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("6 month rule failed");
    }

    [Test]
    public async Task ValidateInterviewAsync_Should_Fail_When_Level_Invalid()
    {
        _candidateContext.Candidates.Add(new Candidate { Id = 1 });

        _requirementContext.Requirements.Add(new Requirement
        {
            Id = 1,
            Project = "Test",
            ClientInterviewRequired = false
        });

        await _candidateContext.SaveChangesAsync();
        await _requirementContext.SaveChangesAsync();

        _repo.Setup(x => x.HasInterviewedInLastSixMonthsAsync(
                1, "Test", It.IsAny<DateTime>()))
            .ReturnsAsync((true, string.Empty));

        var interview = CreateInterview();
        interview.Level = InterviewLevel.Client; // requires CIRequired = true but false

        var result = await _service.ValidateInterviewAsync(interview);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid interview level");
    }

    [Test]
    public async Task ValidateInterviewAsync_Should_Return_True_When_All_Valid()
    {
        _candidateContext.Candidates.Add(new Candidate { Id = 1 });

        _requirementContext.Requirements.Add(new Requirement
        {
            Id = 1,
            Project = "Test",
            ClientInterviewRequired = true
        });

        await _candidateContext.SaveChangesAsync();
        await _requirementContext.SaveChangesAsync();

        _repo.Setup(x => x.HasInterviewedInLastSixMonthsAsync(
                1, "Test", It.IsAny<DateTime>()))
            .ReturnsAsync((true, string.Empty));

        var interview = CreateInterview();

        var result = await _service.ValidateInterviewAsync(interview);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    // ================= ValidateInterviewLevelAsync =================

    [Test]
    public async Task ValidateInterviewLevelAsync_Should_Return_False_When_Client_But_Not_Required()
    {
        var result = await _service.ValidateInterviewLevelAsync(
            (int)InterviewLevel.Client,
            false);

        result.Should().BeFalse();
    }

    [Test]
    public async Task ValidateInterviewLevelAsync_Should_Return_True_When_Valid_Level()
    {
        var result = await _service.ValidateInterviewLevelAsync(
            (int)InterviewLevel.Internal,
            false);

        result.Should().BeTrue();
    }

    // ================= HasInterviewedInLastSixMonthsAsync =================

    [Test]
    public async Task HasInterviewedInLastSixMonthsAsync_Should_Call_Repository()
    {
        _repo.Setup(x => x.HasInterviewedInLastSixMonthsAsync(
                1, "Test", It.IsAny<DateTime>()))
            .ReturnsAsync((true, string.Empty));

        var result = await _service.HasInterviewedInLastSixMonthsAsync(
            1, "Test", DateTime.UtcNow);

        result.IsValid.Should().BeTrue();
    }

    // ================= HELPER =================

    private Interview CreateInterview()
    {
        return new Interview
        {
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "A",
            Interviewer = "I",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal
        };
    }
}
