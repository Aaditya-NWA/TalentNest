using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using InterviewService.Contracts.Services;
using InterviewService.Controllers;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using InterviewService.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace InterviewService.Tests.Controllers;

[TestFixture]
[ExcludeFromCodeCoverage]
public class FeedbacksControllerTests
{
    private Mock<IFeedbackService> _feedbackService = null!;
    private Mock<IInterviewService> _interviewService = null!;
    private Mock<IValidator<CreateFeedbackRequest>> _createValidator = null!;
    private Mock<IValidator<UpdateFeedbackRequest>> _updateValidator = null!;
    private Mock<IValidator<SetOutcomeRequest>> _outcomeValidator = null!;
    private FeedbacksController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _feedbackService = new Mock<IFeedbackService>();
        _interviewService = new Mock<IInterviewService>();
        _createValidator = new Mock<IValidator<CreateFeedbackRequest>>();
        _updateValidator = new Mock<IValidator<UpdateFeedbackRequest>>();
        _outcomeValidator = new Mock<IValidator<SetOutcomeRequest>>();

        _controller = new FeedbacksController(
            _feedbackService.Object,
            _interviewService.Object,
            _createValidator.Object,
            _updateValidator.Object,
            _outcomeValidator.Object);
    }

    // ================= CREATE =================

    [Test]
    public async Task CreateFeedback_Should_Return_400_When_Invalid()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("x", "error") }));

        var result = await _controller.CreateFeedback(new CreateFeedbackRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task CreateFeedback_Should_Return_201_When_Valid()
    {
        var response = new FeedbackResponse { Id = 1 };

        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _feedbackService.Setup(s => s.CreateFeedbackAsync(It.IsAny<CreateFeedbackRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.CreateFeedback(new CreateFeedbackRequest());

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task CreateFeedback_Should_Return_404_When_Service_Throws()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _feedbackService.Setup(s => s.CreateFeedbackAsync(It.IsAny<CreateFeedbackRequest>()))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        var result = await _controller.CreateFeedback(new CreateFeedbackRequest());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ================= GET =================

    [Test]
    public async Task GetFeedbackById_Should_Return_404_When_Not_Found()
    {
        _feedbackService.Setup(s => s.GetFeedbackByIdAsync(1))
            .ReturnsAsync((FeedbackResponse?)null);

        var result = await _controller.GetFeedbackById(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetFeedbackById_Should_Return_200_When_Found()
    {
        _feedbackService.Setup(s => s.GetFeedbackByIdAsync(1))
            .ReturnsAsync(new FeedbackResponse { Id = 1 });

        var result = await _controller.GetFeedbackById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ================= UPDATE =================

    [Test]
    public async Task UpdateFeedback_Should_Return_400_When_Invalid()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("x", "error") }));

        var result = await _controller.UpdateFeedback(1, new UpdateFeedbackRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateFeedback_Should_Return_404_When_Not_Found()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _feedbackService.Setup(s => s.UpdateFeedbackAsync(1, It.IsAny<UpdateFeedbackRequest>()))
            .ReturnsAsync((FeedbackResponse?)null);

        var result = await _controller.UpdateFeedback(1, new UpdateFeedbackRequest());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task UpdateFeedback_Should_Return_200_When_Updated()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateFeedbackRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _feedbackService.Setup(s => s.UpdateFeedbackAsync(1, It.IsAny<UpdateFeedbackRequest>()))
            .ReturnsAsync(new FeedbackResponse { Id = 1 });

        var result = await _controller.UpdateFeedback(1, new UpdateFeedbackRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ================= DELETE =================

    [Test]
    public async Task DeleteFeedback_Should_Return_404_When_Not_Found()
    {
        _feedbackService.Setup(s => s.DeleteFeedbackAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.DeleteFeedback(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteFeedback_Should_Return_204_When_Deleted()
    {
        _feedbackService.Setup(s => s.DeleteFeedbackAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteFeedback(1);

        result.Should().BeOfType<NoContentResult>();
    }

    // ================= SET OUTCOME =================

    [Test]
    public async Task SetInterviewOutcome_Should_Return_400_When_Invalid()
    {
        _outcomeValidator.Setup(v => v.ValidateAsync(It.IsAny<SetOutcomeRequest>(), default))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("x", "error") }));

        var result = await _controller.SetInterviewOutcome(1, new SetOutcomeRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task SetInterviewOutcome_Should_Return_200_When_Success()
    {
        _outcomeValidator.Setup(v => v.ValidateAsync(It.IsAny<SetOutcomeRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _interviewService.Setup(s => s.SetInterviewOutcomeAsync(
                1,
                It.IsAny<InterviewOutcome>(),
                It.IsAny<DecisionMaker>()))
            .ReturnsAsync(new InterviewResponse { Id = 1 });

        var result = await _controller.SetInterviewOutcome(
            1,
            new SetOutcomeRequest
            {
                Outcome = InterviewOutcome.Selected,
                DecisionMaker = DecisionMaker.Internal
            });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task SetInterviewOutcome_Should_Return_404_When_Not_Found()
    {
        _outcomeValidator.Setup(v => v.ValidateAsync(It.IsAny<SetOutcomeRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _interviewService.Setup(s => s.SetInterviewOutcomeAsync(
                1,
                It.IsAny<InterviewOutcome>(),
                It.IsAny<DecisionMaker>()))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        var result = await _controller.SetInterviewOutcome(
            1,
            new SetOutcomeRequest
            {
                Outcome = InterviewOutcome.Selected,
                DecisionMaker = DecisionMaker.Internal
            });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ================= GET OUTCOME =================

    [Test]
    public async Task GetInterviewOutcome_Should_Return_404_When_Not_Found()
    {
        _interviewService.Setup(s => s.GetInterviewByIdAsync(1))
            .ReturnsAsync((InterviewResponse?)null);

        var result = await _controller.GetInterviewOutcome(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetInterviewOutcome_Should_Return_200_When_Found()
    {
        _interviewService.Setup(s => s.GetInterviewByIdAsync(1))
            .ReturnsAsync(new InterviewResponse
            {
                Id = 1,
                CandidateId = 2,
                Project = "P"
            });

        var result = await _controller.GetInterviewOutcome(1);

        result.Should().BeOfType<OkObjectResult>();
    }
}
