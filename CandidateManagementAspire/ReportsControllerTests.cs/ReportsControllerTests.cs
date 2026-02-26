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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /candidate  — merged: summary + paged stats + all-candidates detail
    // ════════════════════════════════════════════════════════════════════════

    #region Candidate (merged)

    [Test]
    public async Task GetCandidateReport_ShouldReturnOk_WithDefaultParams()
    {
        // Defaults: page=1, pageSize=1000, detailPage=1, detailPageSize=50
        var response = new CombinedCandidateReportResponse();

        _mockManager
            .Setup(x => x.GetCombinedCandidateReportAsync(1, 1000, 1, 50))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetCombinedCandidateReportAsync(1, 1000, 1, 50), Times.Once);
    }

    [Test]
    public async Task GetCandidateReport_ShouldPassCustomStatsPage()
    {
        var response = new CombinedCandidateReportResponse();

        _mockManager
            .Setup(x => x.GetCombinedCandidateReportAsync(2, 500, 1, 50))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport(page: 2, pageSize: 500);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetCombinedCandidateReportAsync(2, 500, 1, 50), Times.Once);
    }

    [Test]
    public async Task GetCandidateReport_ShouldPassCustomDetailPage()
    {
        var response = new CombinedCandidateReportResponse();

        _mockManager
            .Setup(x => x.GetCombinedCandidateReportAsync(1, 1000, 3, 25))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport(detailPage: 3, detailPageSize: 25);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetCombinedCandidateReportAsync(1, 1000, 3, 25), Times.Once);
    }

    [Test]
    public async Task GetCandidateReport_ShouldPassAllCustomParams()
    {
        var response = new CombinedCandidateReportResponse();

        _mockManager
            .Setup(x => x.GetCombinedCandidateReportAsync(2, 200, 4, 10))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateReport(
            page: 2, pageSize: 200, detailPage: 4, detailPageSize: 10);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.GetCombinedCandidateReportAsync(2, 200, 4, 10), Times.Once);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    //  GET /candidate/{id}
    // ════════════════════════════════════════════════════════════════════════

    #region Candidate Detailed

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnBadRequest_WhenIdNegative()
    {
        var result = await _controller.GetCandidateDetailedReport(-1);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockManager.Verify(x =>
            x.GetCandidateDetailedReportAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnNotFound_WhenNull()
    {
        _mockManager
            .Setup(x => x.GetCandidateDetailedReportAsync(5))
            .ReturnsAsync((CandidateDetailedReportResponse?)null);

        var result = await _controller.GetCandidateDetailedReport(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnOk_WhenFound()
    {
        var response = new CandidateDetailedReportResponse();

        _mockManager
            .Setup(x => x.GetCandidateDetailedReportAsync(5))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateDetailedReport(5);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    [Test]
    public async Task GetCandidateDetailedReport_ShouldReturnOk_WhenIdIsZero()
    {
        // id=0 is not negative → should reach the manager
        var response = new CandidateDetailedReportResponse();

        _mockManager
            .Setup(x => x.GetCandidateDetailedReportAsync(0))
            .ReturnsAsync(response);

        var result = await _controller.GetCandidateDetailedReport(0);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    //  GET /interview-validation
    // ════════════════════════════════════════════════════════════════════════

    #region Interview Validation

    [Test]
    public async Task GetInterviewValidationReport_ShouldReturnOk()
    {
        var response = new InterviewValidationReportResponse();

        _mockManager
            .Setup(x => x.GetInterviewValidationReportAsync())
            .ReturnsAsync(response);

        var result = await _controller.GetInterviewValidationReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    //  GET /requirement-fulfillment
    // ════════════════════════════════════════════════════════════════════════

    #region Requirement Fulfillment

    [Test]
    public async Task GetRequirementFulfillmentReport_ShouldReturnOk_WithDefaultParams()
    {
        var response = new RequirementFulfillmentPagedResponse();

        _mockManager
            .Setup(x => x.GetRequirementFulfillmentPagedAsync(1, 20))
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

        _mockManager
            .Setup(x => x.GetRequirementFulfillmentPagedAsync(2, 10))
            .ReturnsAsync(response);

        var result = await _controller.GetRequirementFulfillmentReport(2, 10);

        result.Should().BeOfType<OkObjectResult>();
        _mockManager.Verify(x =>
            x.GetRequirementFulfillmentPagedAsync(2, 10), Times.Once);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    //  GET /outcomes
    // ════════════════════════════════════════════════════════════════════════

    #region Outcome

    [Test]
    public async Task GetOutcomeReport_ShouldReturnOk()
    {
        var response = new OutcomeReportResponse();

        _mockManager
            .Setup(x => x.GetOutcomeReportAsync())
            .ReturnsAsync(response);

        var result = await _controller.GetOutcomeReport();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    //  GET /performance
    // ════════════════════════════════════════════════════════════════════════

    #region Performance

    [Test]
    public async Task RunPerformanceTest_ShouldReturnOk_WithDefaultRequestCount()
    {
        var response = new PerformanceReportResponse();

        _mockManager
            .Setup(x => x.RunPerformanceTestAsync(20))
            .ReturnsAsync(response);

        var result = await _controller.RunPerformanceTest();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.RunPerformanceTestAsync(20), Times.Once);
    }

    [Test]
    public async Task RunPerformanceTest_ShouldPassCustomRequestCount()
    {
        var response = new PerformanceReportResponse();

        _mockManager
            .Setup(x => x.RunPerformanceTestAsync(10))
            .ReturnsAsync(response);

        var result = await _controller.RunPerformanceTest(10);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        _mockManager.Verify(x =>
            x.RunPerformanceTestAsync(10), Times.Once);
    }

    #endregion
}