using Bogus;
using FluentAssertions;
using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.Models;
using InterviewService.Models.Enums;
using InterviewService.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;


namespace InterviewService.Tests.Services;

[TestFixture]
[ExcludeFromCodeCoverage]
public class FeedbackServiceTests
{
    private Mock<IFeedbackRepository> _feedbackRepo = null!;
    private Mock<IInterviewRepository> _interviewRepo = null!;
    private FeedbackService _service = null!;
    private Faker _faker = null!;

    [SetUp]
    public void Setup()
    {
        _feedbackRepo = new Mock<IFeedbackRepository>();
        _interviewRepo = new Mock<IInterviewRepository>();
        _service = new FeedbackService(_feedbackRepo.Object, _interviewRepo.Object);
        _faker = new Faker();
    }

    // ================= CREATE =================

    [Test]
    public async Task CreateFeedbackAsync_Should_Create_When_Interview_Exists()
    {
        var request = new CreateFeedbackRequest
        {
            InterviewId = 1,
            Comments = _faker.Lorem.Sentence(),
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "tester"
        };

        _interviewRepo.Setup(x => x.ExistsAsync(request.InterviewId))
            .ReturnsAsync(true);

        _feedbackRepo.Setup(x => x.CreateAsync(It.IsAny<Feedback>()))
            .ReturnsAsync((Feedback f) =>
            {
                f.Id = 10;
                return f;
            });

        var result = await _service.CreateFeedbackAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.RecommendedOutcomeName.Should().Be(InterviewOutcome.Selected.ToString());
    }

    [Test]
    public async Task CreateFeedbackAsync_Should_Throw_When_Interview_Not_Exists()
    {
        var request = new CreateFeedbackRequest { InterviewId = 99 };

        _interviewRepo.Setup(x => x.ExistsAsync(request.InterviewId))
            .ReturnsAsync(false);

        await FluentActions
            .Invoking(() => _service.CreateFeedbackAsync(request))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    // ================= READ =================

    [Test]
    public async Task GetFeedbackByIdAsync_Should_Return_Null_When_Not_Found()
    {
        _feedbackRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Feedback?)null);

        var result = await _service.GetFeedbackByIdAsync(1);

        result.Should().BeNull();
    }

    [Test]
    public async Task GetFeedbackByIdAsync_Should_Map_Response()
    {
        var feedback = new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "Good",
            RecommendedOutcome = InterviewOutcome.Rejected,
            CreatedBy = "admin"
        };

        _feedbackRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(feedback);

        var result = await _service.GetFeedbackByIdAsync(1);

        result.Should().NotBeNull();
        result!.RecommendedOutcomeName.Should().Be("Rejected");
    }

    // ================= UPDATE =================

    [Test]
    public async Task UpdateFeedbackAsync_Should_Return_Null_When_Not_Found()
    {
        _feedbackRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Feedback?)null);

        var result = await _service.UpdateFeedbackAsync(1, new UpdateFeedbackRequest());

        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateFeedbackAsync_Should_Update_Fields_When_Provided()
    {
        var existing = new Feedback
        {
            Id = 1,
            Comments = "Old",
            RecommendedOutcome = InterviewOutcome.Pending
        };

        _feedbackRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _feedbackRepo.Setup(x => x.UpdateAsync(1, existing))
            .ReturnsAsync(existing);

        var request = new UpdateFeedbackRequest
        {
            Comments = "Updated",
            RecommendedOutcome = InterviewOutcome.Selected
        };

        var result = await _service.UpdateFeedbackAsync(1, request);

        result!.Comments.Should().Be("Updated");
        result.RecommendedOutcomeName.Should().Be("Selected");
    }

    // ================= DELETE =================

    [Test]
    public async Task DeleteFeedbackAsync_Should_Return_Result()
    {
        _feedbackRepo.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteFeedbackAsync(1);

        result.Should().BeTrue();
    }

    // ================= STATISTICS =================

    [Test]
    public async Task GetAverageScoreForInterviewAsync_Should_Return_Value()
    {
        _feedbackRepo.Setup(x => x.GetAverageScoreByInterviewAsync(1))
            .ReturnsAsync(4.2);

        var result = await _service.GetAverageScoreForInterviewAsync(1);

        result.Should().Be(4.2);
    }

    [Test]
    public async Task GetAverageScoresForInterviewsAsync_Should_Return_Dictionary()
    {
        _feedbackRepo.Setup(x => x.GetAverageScoreByInterviewAsync(It.IsAny<int>()))
            .ReturnsAsync(3.5);

        var ids = new List<int> { 1, 2, 3 };

        var result = await _service.GetAverageScoresForInterviewsAsync(ids);

        result.Count.Should().Be(3);
        result.Values.Should().OnlyContain(x => x == 3.5);
    }
    [Test]
    public async Task GetAllFeedbacksAsync_Should_Map_List()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "Test",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "admin"
        }
    };

        _feedbackRepo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(list);

        var result = await _service.GetAllFeedbacksAsync();

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task GetAllFeedbacksAsync_With_Date_Filter_Should_Map_List()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "Test",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "admin"
        }
    };

        _feedbackRepo.Setup(x => x.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(list);

        var result = await _service.GetAllFeedbacksAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task GetFeedbacksByCreatorAsync_Should_Map_List()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "Test",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "admin"
        }
    };

        _feedbackRepo.Setup(x => x.GetByCreatedByAsync("admin"))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByCreatorAsync("admin");

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task GetFeedbacksByInterviewAsync_Should_Map_List()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "Test",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "admin"
        }
    };

        _feedbackRepo.Setup(x => x.GetByInterviewIdAsync(1))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByInterviewAsync(1);

        result.Should().HaveCount(1);
    }
    [Test]
    public async Task GetFeedbacksByCreatorAsync_Should_Return_Empty_When_None()
    {
        _feedbackRepo.Setup(x => x.GetByCreatedByAsync("admin"))
            .ReturnsAsync(new List<Feedback>());

        var result = await _service.GetFeedbacksByCreatorAsync("admin");

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetFeedbacksByInterviewAsync_Should_Return_Empty_When_None()
    {
        _feedbackRepo.Setup(x => x.GetByInterviewIdAsync(1))
            .ReturnsAsync(new List<Feedback>());

        var result = await _service.GetFeedbacksByInterviewAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task UpdateFeedbackAsync_Should_Not_Update_Fields_When_Request_Is_Empty()
    {
        var existing = new Feedback
        {
            Id = 1,
            Comments = "Old",
            RecommendedOutcome = InterviewOutcome.Pending
        };

        var request = new UpdateFeedbackRequest
        {
            Comments = null,
            RecommendedOutcome = null
        };

        _feedbackRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _feedbackRepo.Setup(x => x.UpdateAsync(1, existing)).ReturnsAsync(existing);

        var result = await _service.UpdateFeedbackAsync(1, request);

        result.Should().NotBeNull();
        result!.Comments.Should().Be("Old");
    }
    [Test]
    public async Task GetAllFeedbacksAsync_With_Null_Dates_Should_Work()
    {
        _feedbackRepo
            .Setup(x => x.GetAllAsync(null, null))
            .ReturnsAsync(new List<Feedback>());

        var result = await _service.GetAllFeedbacksAsync(null, null);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task GetFeedbacksByInterviewAsync_Should_Return_Empty_List()
    {
        _feedbackRepo
            .Setup(x => x.GetByInterviewIdAsync(1))
            .ReturnsAsync(new List<Feedback>());

        var result = await _service.GetFeedbacksByInterviewAsync(1);

        result.Should().BeEmpty();
    }
    [Test]
    public async Task UpdateFeedbackAsync_Should_Return_Null_When_Update_Returns_Null()
    {
        var existing = new Feedback
        {
            Id = 1,
            Comments = "Old"
        };

        var request = new UpdateFeedbackRequest
        {
            Comments = "New"
        };

        _feedbackRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _feedbackRepo.Setup(x => x.UpdateAsync(1, existing)).ReturnsAsync((Feedback?)null);

        var result = await _service.UpdateFeedbackAsync(1, request);

        result.Should().BeNull();
    }
    [Test]
    public async Task GetAllFeedbacksAsync_With_Items_Should_Map_All()
    {
        var list = new List<Feedback>
    {
        new Feedback { Id = 1, InterviewId = 1, Comments = "A", RecommendedOutcome = InterviewOutcome.Selected, CreatedBy = "Admin" },
        new Feedback { Id = 2, InterviewId = 1, Comments = "B", RecommendedOutcome = InterviewOutcome.Rejected, CreatedBy = "Admin" }
    };

        _feedbackRepo
            .Setup(x => x.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(list);

        var result = await _service.GetAllFeedbacksAsync(null, null);

        result.Count().Should().Be(2);
    }
    [Test]
    public async Task GetFeedbacksByCreatorAsync_With_Items_Should_Map_All()
    {
        var list = new List<Feedback>
    {
        new Feedback { Id = 1, InterviewId = 1, Comments = "A", RecommendedOutcome = InterviewOutcome.Selected, CreatedBy = "Admin" }
    };

        _feedbackRepo
            .Setup(x => x.GetByCreatedByAsync("Admin"))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByCreatorAsync("Admin");

        result.Should().HaveCount(1);
    }
    [Test]
    public async Task GetFeedbacksByInterviewAsync_With_Items_Should_Map_All()
    {
        var list = new List<Feedback>
    {
        new Feedback { Id = 1, InterviewId = 1, Comments = "A", RecommendedOutcome = InterviewOutcome.Selected, CreatedBy = "Admin" }
    };

        _feedbackRepo
            .Setup(x => x.GetByInterviewIdAsync(1))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByInterviewAsync(1);

        result.Should().HaveCount(1);
    }
    [Test]
    public async Task GetAllFeedbacksAsync_Should_Enumerate_All_Items()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "A",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "Admin"
        },
        new Feedback
        {
            Id = 2,
            InterviewId = 1,
            Comments = "B",
            RecommendedOutcome = InterviewOutcome.Rejected,
            CreatedBy = "Admin"
        }
    };

        _feedbackRepo
            .Setup(x => x.GetAllAsync(null, null))
            .ReturnsAsync(list);

        var result = await _service.GetAllFeedbacksAsync(null, null);

        var materialized = result.ToList();   // 👈 THIS IS THE KEY

        materialized.Count.Should().Be(2);
    }
    [Test]
    public async Task GetFeedbacksByCreatorAsync_Should_Enumerate()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "A",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "Admin"
        }
    };

        _feedbackRepo
            .Setup(x => x.GetByCreatedByAsync("Admin"))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByCreatorAsync("Admin");

        var materialized = result.ToList();  // 👈 FORCE EXECUTION

        materialized.Should().HaveCount(1);
    }
    [Test]
    public async Task GetFeedbacksByInterviewAsync_Should_Enumerate()
    {
        var list = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            InterviewId = 1,
            Comments = "A",
            RecommendedOutcome = InterviewOutcome.Selected,
            CreatedBy = "Admin"
        }
    };

        _feedbackRepo
            .Setup(x => x.GetByInterviewIdAsync(1))
            .ReturnsAsync(list);

        var result = await _service.GetFeedbacksByInterviewAsync(1);

        var materialized = result.ToList();  // 👈 FORCE EXECUTION

        materialized.Should().HaveCount(1);
    }
    [Test]
    public void GetAllFeedbacksAsync_WithDates_Should_Throw_When_Repo_Fails()
    {
        _feedbackRepo
            .Setup(x => x.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("DB Error"));

        Func<Task> act = async () =>
            await _service.GetAllFeedbacksAsync(null, null);

        act.Should().ThrowAsync<Exception>();
    }
    [Test]
    public void GetFeedbacksByCreatorAsync_Should_Throw_When_Repo_Fails()
    {
        _feedbackRepo
            .Setup(x => x.GetByCreatedByAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB Error"));

        Func<Task> act = async () =>
            await _service.GetFeedbacksByCreatorAsync("Admin");

        act.Should().ThrowAsync<Exception>();
    }
    [Test]
    public void GetFeedbacksByInterviewAsync_Should_Throw_When_Repo_Fails()
    {
        _feedbackRepo
            .Setup(x => x.GetByInterviewIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB Error"));

        Func<Task> act = async () =>
            await _service.GetFeedbacksByInterviewAsync(1);

        act.Should().ThrowAsync<Exception>();
    }













}
