using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using InterviewService.Contracts.Services;
using InterviewService.Controllers;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace InterviewService.Tests.Controllers;

[TestFixture]
[ExcludeFromCodeCoverage]
public class InterviewsControllerTests
{
    private Mock<IInterviewService> _service = null!;
    private Mock<IValidator<CreateInterviewRequest>> _createValidator = null!;
    private Mock<IValidator<UpdateInterviewRequest>> _updateValidator = null!;
    private InterviewsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _service = new Mock<IInterviewService>();
        _createValidator = new Mock<IValidator<CreateInterviewRequest>>();
        _updateValidator = new Mock<IValidator<UpdateInterviewRequest>>();

        _controller = new InterviewsController(
            _service.Object,
            _createValidator.Object,
            _updateValidator.Object);
    }

    // ================= CREATE =================

    [Test]
    public async Task CreateInterview_Should_Return_400_When_Invalid()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult(
                new[] { new ValidationFailure("x", "error") }));

        var result = await _controller.CreateInterview(new CreateInterviewRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task CreateInterview_Should_Return_201_When_Valid()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _service.Setup(s => s.CreateInterviewAsync(It.IsAny<CreateInterviewRequest>()))
            .ReturnsAsync(new InterviewResponse { Id = 1 });

        var result = await _controller.CreateInterview(new CreateInterviewRequest());

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task CreateInterview_Should_Return_409_When_Service_Throws()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _service.Setup(s => s.CreateInterviewAsync(It.IsAny<CreateInterviewRequest>()))
            .ThrowsAsync(new InvalidOperationException("conflict"));

        var result = await _controller.CreateInterview(new CreateInterviewRequest());

        result.Should().BeOfType<ConflictObjectResult>();
    }

    // ================= GET BY ID =================

    [Test]
    public async Task GetInterviewById_Should_Return_404_When_Not_Found()
    {
        _service.Setup(s => s.GetInterviewByIdAsync(1))
            .ReturnsAsync((InterviewResponse?)null);

        var result = await _controller.GetInterviewById(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetInterviewById_Should_Return_200_When_Found()
    {
        _service.Setup(s => s.GetInterviewByIdAsync(1))
            .ReturnsAsync(new InterviewResponse { Id = 1 });

        var result = await _controller.GetInterviewById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ================= GET ALL =================

    [Test]
    public async Task GetAllInterviews_Should_Return_200()
    {
        _service.Setup(s => s.GetAllInterviewsAsync())
            .ReturnsAsync(new List<InterviewResponse>
            {
                new InterviewResponse { Id = 1 }
            });

        var result = await _controller.GetAllInterviews();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ================= UPDATE =================

    [Test]
    public async Task UpdateInterview_Should_Return_400_When_Invalid()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult(
                new[] { new ValidationFailure("x", "error") }));

        var result = await _controller.UpdateInterview(1, new UpdateInterviewRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateInterview_Should_Return_404_When_Not_Found()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _service.Setup(s => s.UpdateInterviewAsync(1, It.IsAny<UpdateInterviewRequest>()))
            .ReturnsAsync((InterviewResponse?)null);

        var result = await _controller.UpdateInterview(1, new UpdateInterviewRequest());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task UpdateInterview_Should_Return_200_When_Success()
    {
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateInterviewRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        _service.Setup(s => s.UpdateInterviewAsync(1, It.IsAny<UpdateInterviewRequest>()))
            .ReturnsAsync(new InterviewResponse { Id = 1 });

        var result = await _controller.UpdateInterview(1, new UpdateInterviewRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ================= DELETE =================

    [Test]
    public async Task DeleteInterview_Should_Return_404_When_Not_Found()
    {
        _service.Setup(s => s.DeleteInterviewAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.DeleteInterview(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteInterview_Should_Return_204_When_Deleted()
    {
        _service.Setup(s => s.DeleteInterviewAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteInterview(1);

        result.Should().BeOfType<NoContentResult>();
    }
}
