using Bogus;
using FluentAssertions;
using FluentAssertions.Common;
using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.Models;
using InterviewService.Models.Enums;
using InterviewService.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;



namespace InterviewService.Tests.Services;

using InterviewSvc = InterviewService.Services.InterviewService;


[TestFixture]
[ExcludeFromCodeCoverage]
public class InterviewServiceTests
{
    private Mock<IInterviewRepository> _repo = null!;
    private Mock<IInterviewValidationService> _validation = null!;
    private InterviewService.Services.InterviewService _service = null!;
    private Faker _faker = null!;

    [SetUp]
    public void Setup()
    {
        _repo = new Mock<IInterviewRepository>();
        _validation = new Mock<IInterviewValidationService>();
        _service = new InterviewService.Services.InterviewService(_repo.Object, _validation.Object);
        _faker = new Faker();
        _repo = new Mock<IInterviewRepository>();
        _validation = new Mock<IInterviewValidationService>();
        _service = new InterviewSvc(_repo.Object, _validation.Object);
    }

    // ================= CREATE =================

    [Test]
    public async Task CreateInterviewAsync_Should_Create_When_Valid()
    {
        var request = new CreateInterviewRequest
        {
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            InterviewDate = DateTime.UtcNow,
            Level = (int)InterviewLevel.Internal
        };

        _validation.Setup(x => x.ValidateInterviewAsync(It.IsAny<Interview>()))
            .ReturnsAsync((true, string.Empty));

        _repo.Setup(x => x.CreateAsync(It.IsAny<Interview>()))
            .ReturnsAsync((Interview i) =>
            {
                i.Id = 10;
                return i;
            });

        var result = await _service.CreateInterviewAsync(request);

        result.Id.Should().Be(10);
        result.Project.Should().Be("Test");
        result.Level.Should().Be((int)InterviewLevel.Internal);
    }

    [Test]
    public async Task CreateInterviewAsync_Should_Throw_When_Invalid()
    {
        var request = new CreateInterviewRequest
        {
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            InterviewDate = DateTime.UtcNow,
            Level = (int)InterviewLevel.Internal
        };

        _validation.Setup(x => x.ValidateInterviewAsync(It.IsAny<Interview>()))
            .ReturnsAsync((false, "Validation failed"));

        await FluentActions
            .Invoking(() => _service.CreateInterviewAsync(request))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Validation failed");
    }

    // ================= READ =================

    [Test]
    public async Task GetInterviewByIdAsync_Should_Return_Null_When_Not_Found()
    {
        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Interview?)null);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().BeNull();
    }

    [Test]
    public async Task GetInterviewByIdAsync_Should_Map_Response()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 2,
            RequirementId = 3,
            Project = "P",
            Account = "A",
            Interviewer = "I",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Client,
            FinalOutcome = InterviewOutcome.Pending,
            DecisionMaker = DecisionMaker.NotApplicable
        };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(interview);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().NotBeNull();
        result!.Level.Should().Be((int)InterviewLevel.Client);
        result.FinalOutcome.Should().Be(InterviewOutcome.Pending);
    }

    [Test]
    public async Task GetAllInterviewsAsync_Should_Map_List()
    {
        var interviews = new List<Interview>
        {
            new Interview
            {
                Id = 1,
                CandidateId = 1,
                RequirementId = 1,
                Project = "P",
                Account = "A",
                Interviewer = "I",
                InterviewDate = DateTime.UtcNow,
                Level = InterviewLevel.Internal
            }
        };

        _repo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(interviews);

        var result = await _service.GetAllInterviewsAsync();

        result.Should().HaveCount(1);
    }

    // ================= UPDATE =================

    [Test]
    public async Task UpdateInterviewAsync_Should_Return_Null_When_Not_Found()
    {
        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Interview?)null);

        var result = await _service.UpdateInterviewAsync(1, new UpdateInterviewRequest());

        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateInterviewAsync_Should_Update_Provided_Fields()
    {
        var existing = new Interview
        {
            Id = 1,
            Project = "Old",
            Account = "OldAcc",
            Interviewer = "OldInt",
            InterviewDate = DateTime.UtcNow.AddDays(-1),
            Level = InterviewLevel.Internal
        };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _repo.Setup(x => x.UpdateAsync(1, existing))
            .ReturnsAsync(existing);

        var request = new UpdateInterviewRequest
        {
            Project = "NewProject",
            Level = (int)InterviewLevel.Client
        };

        var result = await _service.UpdateInterviewAsync(1, request);

        result!.Project.Should().Be("NewProject");
        result.Level.Should().Be((int)InterviewLevel.Client);
    }

    // ================= OUTCOME =================

    [Test]
    public async Task SetInterviewOutcomeAsync_Should_Update_When_Found()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "P",
            Account = "A",
            Interviewer = "I",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal
        };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(interview);

        _repo.Setup(x => x.UpdateAsync(1, interview))
            .ReturnsAsync(interview);

        var result = await _service.SetInterviewOutcomeAsync(
            1,
            InterviewOutcome.Selected,
            DecisionMaker.Internal);

        result.FinalOutcome.Should().Be(InterviewOutcome.Selected);
        result.DecisionMaker.Should().Be(DecisionMaker.Internal);
    }

    [Test]
    public async Task SetInterviewOutcomeAsync_Should_Throw_When_Not_Found()
    {
        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Interview?)null);

        await FluentActions
            .Invoking(() => _service.SetInterviewOutcomeAsync(
                1,
                InterviewOutcome.Selected,
                DecisionMaker.Internal))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    // ================= DELETE =================

    [Test]
    public async Task DeleteInterviewAsync_Should_Return_Result()
    {
        _repo.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteInterviewAsync(1);

        result.Should().BeTrue();
    }
    [Test]
    public async Task UpdateInterviewAsync_Should_Update_All_Fields_When_Provided()
    {
        var existing = new Interview
        {
            Id = 1,
            Project = "Old",
            Account = "Old",
            Interviewer = "Old",
            InterviewDate = DateTime.UtcNow.AddDays(-2),
            Level = InterviewLevel.Internal
        };

        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _repo.Setup(x => x.UpdateAsync(1, existing)).ReturnsAsync(existing);

        var request = new UpdateInterviewRequest
        {
            Project = "New",
            Account = "NewAcc",
            Interviewer = "NewInt",
            InterviewDate = DateTime.UtcNow,
            Level = (int)InterviewLevel.Client
        };

        var result = await _service.UpdateInterviewAsync(1, request);

        result!.Project.Should().Be("New");
        result.Account.Should().Be("NewAcc");
        result.Level.Should().Be((int)InterviewLevel.Client);
    }
    [Test]
    public async Task GetInterviewByIdAsync_Should_Map_Feedbacks()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "P",
            Account = "A",
            Interviewer = "I",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = 10,
                InterviewId = 1,
                Comments = "Good",
                RecommendedOutcome = InterviewOutcome.Selected,
                CreatedBy = "admin"
            }
        }
        };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(interview);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().NotBeNull();
    }
    [Test]
    public async Task SetInterviewOutcomeAsync_Should_Map_Feedbacks()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "P",
            Account = "A",
            Interviewer = "I",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = 100,
                InterviewId = 1,
                Comments = "Excellent",
                RecommendedOutcome = InterviewOutcome.Selected,
                CreatedBy = "admin"
            }
        }
        };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(interview);

        _repo.Setup(x => x.UpdateAsync(1, interview))
            .ReturnsAsync(interview);

        var result = await _service.SetInterviewOutcomeAsync(
            1,
            InterviewOutcome.Selected,
            DecisionMaker.Internal);

        result.Should().NotBeNull();
    }
    [Test]

    public async Task UpdateInterviewAsync_Should_Not_Update_Any_Field_When_Request_Is_Empty()
    {
        var existing = new Interview
        {
            Id = 1,
            Project = "OldProject",
            Account = "OldAccount",
            Interviewer = "OldInterviewer",
            InterviewDate = DateTime.UtcNow.AddDays(-1),
            Level = InterviewLevel.Internal
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(existing);

        _repo.Setup(x => x.UpdateAsync(1, existing))
             .ReturnsAsync(existing);

        var request = new UpdateInterviewRequest();

        await _service.UpdateInterviewAsync(1, request);

        _repo.Verify(x => x.UpdateAsync(1,
            It.Is<Interview>(i =>
                i.Project == "OldProject" &&
                i.Account == "OldAccount" &&
                i.Interviewer == "OldInterviewer"
            )),
        Times.Once);
    }

    [Test]
    public async Task GetInterviewByIdAsync_Should_Handle_Null_Feedbacks()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = null // IMPORTANT
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(interview);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }
    [Test]
    public async Task SetInterviewOutcomeAsync_Should_Map_Feedbacks_When_Present()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = 10,
                InterviewId = 1,
                Comments = "Good",
                RecommendedOutcome = InterviewOutcome.Selected,
                CreatedBy = "Admin"
            }
        }
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(interview);

        _repo.Setup(x => x.UpdateAsync(1, It.IsAny<Interview>()))
             .ReturnsAsync(interview);

        var result = await _service.SetInterviewOutcomeAsync(
            1,
            InterviewOutcome.Selected,
            DecisionMaker.Client);

        result.Should().NotBeNull();
        result.DecisionMaker.Should().Be(DecisionMaker.Client);
    }
    [Test]
    public async Task GetAllInterviewsAsync_Should_Enumerate_Results()
    {
        var interviews = new List<Interview>
    {
        new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal
        }
    };

        _repo.Setup(x => x.GetAllAsync())
             .ReturnsAsync(interviews);

        var result = await _service.GetAllInterviewsAsync();

        var list = result.ToList(); // FORCE MoveNext()

        list.Should().HaveCount(1);
    }
    [Test]
    public async Task UpdateInterviewAsync_Should_Return_Null_When_Update_Returns_Null()
    {
        var existing = new Interview
        {
            Id = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(existing);

        _repo.Setup(x => x.UpdateAsync(1, existing))
             .ReturnsAsync((Interview?)null); // IMPORTANT

        var request = new UpdateInterviewRequest
        {
            Project = "NewProject"
        };

        var result = await _service.UpdateInterviewAsync(1, request);

        result.Should().BeNull();
    }
    [Test]
    public async Task GetInterviewByIdAsync_Should_Map_Feedbacks_When_Present()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = 100,
                InterviewId = 1,
                Comments = "Good",
                RecommendedOutcome = InterviewOutcome.Selected,
                CreatedBy = "Admin"
            }
        }
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(interview);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }
    [Test]
    public async Task GetInterviewByIdAsync_Should_Handle_Empty_Feedback_List()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>() // empty list
        };

        _repo.Setup(x => x.GetByIdAsync(1))
             .ReturnsAsync(interview);

        var result = await _service.GetInterviewByIdAsync(1);

        result.Should().NotBeNull();
    }
    
    [Test]
    public void MapToResponse_Should_Execute_All_Feedback_Branches()
    {

        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            InterviewDate = DateTime.UtcNow,
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    Id = 5,
                    InterviewId = 1,
                    Comments = "Nice",
                    RecommendedOutcome = InterviewOutcome.Selected,
                    CreatedBy = "Admin"
                }
            }
        };

        var method = typeof(InterviewService.Services.InterviewService)

            .GetMethod("MapToResponse",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { interview });

        result.Should().NotBeNull();
    }
    [Test]
    public async Task SetInterviewOutcome_Should_Handle_Null_Feedbacks()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            Level = InterviewLevel.Internal,
            FinalOutcome = InterviewOutcome.Pending,
            DecisionMaker = DecisionMaker.NotApplicable,
            Feedbacks = null // <-- important
        };

        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(interview);
        _repo.Setup(x => x.UpdateAsync(1, interview)).ReturnsAsync(interview);

        var result = await _service.SetInterviewOutcomeAsync(
            1,
            InterviewOutcome.Selected,
            DecisionMaker.Internal);

        result.Should().NotBeNull();
    }
    [Test]
    public async Task SetInterviewOutcome_Should_Map_Feedbacks_When_Present()
    {
        var interview = new Interview
        {
            Id = 1,
            CandidateId = 1,
            RequirementId = 1,
            Project = "Test",
            Account = "Acc",
            Interviewer = "John",
            Level = InterviewLevel.Internal,
            Feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = 10,
                InterviewId = 1,
                Comments = "Good",
                RecommendedOutcome = InterviewOutcome.Selected,
                CreatedBy = "Admin"
            }
        }
        };

        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(interview);
        _repo.Setup(x => x.UpdateAsync(1, interview)).ReturnsAsync(interview);

        var result = await _service.SetInterviewOutcomeAsync(
            1,
            InterviewOutcome.Selected,
            DecisionMaker.Internal);

        result.Should().NotBeNull();
    }
    [Test]
    public void GetInterviewById_ShouldThrow_WhenIdNegative()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        var service = new InterviewService.Services.InterviewService(repo.Object, validation.Object);


        Action act = () => service.GetInterviewByIdAsync(-1).GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public void UpdateInterview_ShouldThrow_WhenIdNegative()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        var service = new InterviewSvc(repo.Object, validation.Object);

        Action act = () => service.UpdateInterviewAsync(-1, new UpdateInterviewRequest())
                                    .GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public void SetOutcome_ShouldThrow_WhenIdNegative()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        var service = new InterviewSvc(repo.Object, validation.Object);

        Action act = () => service.SetInterviewOutcomeAsync(
                                -1,
                                InterviewOutcome.Selected,
                                DecisionMaker.Internal)
                            .GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public void SetOutcome_ShouldThrow_WhenInterviewNotFound()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Interview?)null);

        var service = new InterviewSvc(repo.Object, validation.Object);

        Action act = () => service.SetInterviewOutcomeAsync(
                                1,
                                InterviewOutcome.Selected,
                                DecisionMaker.Internal)
                            .GetAwaiter().GetResult();

        act.Should().Throw<KeyNotFoundException>();
    }
    [Test]
    public void DeleteInterview_ShouldThrow_WhenIdNegative()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        var service = new InterviewSvc(repo.Object, validation.Object);

        Action act = () => service.DeleteInterviewAsync(-1)
                                  .GetAwaiter().GetResult();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id cannot be negative");
    }
    [Test]
    public void CreateInterview_ShouldThrow_WhenValidationFails()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        validation.Setup(v => v.ValidateInterviewAsync(It.IsAny<Interview>()))
            .ReturnsAsync((false, "Validation failed"));

        var service = new InterviewSvc(repo.Object, validation.Object);

        var request = new CreateInterviewRequest
        {
            CandidateId = 1,
            RequirementId = 1,
            Project = "Proj",
            Account = "Acc",
            Interviewer = "Int",
            InterviewDate = DateTime.Today,
            Level = 1
        };

        Action act = () => service.CreateInterviewAsync(request)
                                  .GetAwaiter().GetResult();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Validation failed");
    }
    [Test]
    public async Task UpdateInterview_ShouldReturnNull_WhenNotFound()
    {
        var repo = new Mock<IInterviewRepository>();
        var validation = new Mock<IInterviewValidationService>();

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Interview?)null);

        var service = new InterviewSvc(repo.Object, validation.Object);

        var result = await service.UpdateInterviewAsync(1, new UpdateInterviewRequest());

        result.Should().BeNull();
    }









}











