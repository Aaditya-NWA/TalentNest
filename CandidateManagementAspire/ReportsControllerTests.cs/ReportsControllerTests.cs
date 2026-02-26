using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ReportService.Controllers;
using ReportService.DTOs;
using ReportService.Services;
using System.Diagnostics.CodeAnalysis;

namespace ReportService.Tests.Controllers;

[TestFixture]
[ExcludeFromCodeCoverage]
public class ReportsControllerTests
{
    private Mock<IReportManager> _mockManager;
    private ReportsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockManager = new Mock<IReportManager>();
        _controller = new ReportsController(_mockManager.Object);
    }

    #region Summary

    [Test]
    public async Task GetSummary_ShouldReturnOk_WithResponse()
    {
        var response = new ReportSummaryResponse();
        _mockManager.Setup(x => x.GetSystemSummaryAsync())
            .ReturnsAsync(response);

        var result = await _controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    #region Candidate Paged

    [Test]
    public async Task GetCandidateReport_ShouldReturnOk_WithPagedResponse()
    {
        var response = new CandidateReportPagedResponse();

        _mockManager.Setup(x =>
                x.GetCandidateReportPagedAsync(1, 1000))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetCandidateReportPagedAsync(1, 1000), Times.Once);
    }

    [Test]
    public async Task GetCandidateReport_ShouldPassCustomPagination()
    {
        var response = new CandidateReportPagedResponse();

        _mockManager.Setup(x =>
                x.GetCandidateReportPagedAsync(2, 50))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport(2, 50);

        result.Should().BeOfType<OkObjectResult>();
        _mockManager.Verify(x =>
            x.GetCandidateReportPagedAsync(2, 50), Times.Once);
    }

    #endregion

    #region Interview Validation

    [Test]
    public async Task GetInterviewValidationReport_ShouldReturnOk()
    {
        var response = new InterviewValidationReportResponse();

        _mockManager.Setup(x =>
                x.GetInterviewValidationReportAsync())
            .ReturnsAsync(response);

        var result = await _controller.GetInterviewValidationReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    #region Requirement Fulfillment

    [Test]
    public async Task GetRequirementFulfillmentReport_ShouldReturnOk()
    {
        var response = new RequirementFulfillmentPagedResponse();

        _mockManager.Setup(x =>
                x.GetRequirementFulfillmentPagedAsync(1, 20))
            .ReturnsAsync(response);

        var result = await _controller.GetRequirementFulfillmentReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetRequirementFulfillmentPagedAsync(1, 20), Times.Once);
    }

    [Test]
    public async Task GetRequirementFulfillmentReport_ShouldPassCustomPagination()
    {
        var response = new RequirementFulfillmentPagedResponse();

        _mockManager.Setup(x =>
                x.GetRequirementFulfillmentPagedAsync(2, 10))
            .ReturnsAsync(response);

        var result = await _controller.GetRequirementFulfillmentReport(2, 10);

        result.Should().BeOfType<OkObjectResult>();
        _mockManager.Verify(x =>
            x.GetRequirementFulfillmentPagedAsync(2, 10), Times.Once);
    }

    #endregion

    #region Outcome

    [Test]
    public async Task GetOutcomeReport_ShouldReturnOk()
    {
        var response = new OutcomeReportResponse();

        _mockManager.Setup(x =>
                x.GetOutcomeReportAsync())
            .ReturnsAsync(response);

        var result = await _controller.GetOutcomeReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    #region Performance

    [Test]
    public async Task RunPerformanceTest_ShouldReturnOk_WithDefaultValue()
    {
        var response = new PerformanceReportResponse();

        _mockManager.Setup(x =>
                x.RunPerformanceTestAsync(20))
            .ReturnsAsync(response);

        var result = await _controller.RunPerformanceTest();

        result.Should().BeOfType<OkObjectResult>();
        _mockManager.Verify(x =>
            x.RunPerformanceTestAsync(20), Times.Once);
    }

    [Test]
    public async Task RunPerformanceTest_ShouldPassCustomRequestCount()
    {
        var response = new PerformanceReportResponse();

        _mockManager.Setup(x =>
                x.RunPerformanceTestAsync(10))
            .ReturnsAsync(response);

        var result = await _controller.RunPerformanceTest(10);

        result.Should().BeOfType<OkObjectResult>();
        _mockManager.Verify(x =>
            x.RunPerformanceTestAsync(10), Times.Once);
    }

    #endregion

    #region Candidate Detailed

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnBadRequest_WhenIdNegative()
    {
        var result = await _controller.GetCandidateDetailedReport(-1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnNotFound_WhenNull()
    {
        _mockManager.Setup(x =>
                x.GetCandidateDetailedReportAsync(5))
            .ReturnsAsync((CandidateDetailedReportResponse?)null);

        var result = await _controller.GetCandidateDetailedReport(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnOk_WhenFound()
    {
        var response = new CandidateDetailedReportResponse();

        _mockManager.Setup(x =>
                x.GetCandidateDetailedReportAsync(5))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateDetailedReport(5);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion
}