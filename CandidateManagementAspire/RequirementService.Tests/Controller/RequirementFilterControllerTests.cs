using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using RequirementService.Contracts.Clients;
using RequirementService.Controllers;
using RequirementService.DTOs.External;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RequirementService.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class RequirementFilterControllerTests
    {
        private Mock<ICandidateClient> _mockClient;
        private RequirementFilterController _controller;

        [SetUp]
        public void Setup()
        {
            _mockClient = new Mock<ICandidateClient>();
            _controller = new RequirementFilterController(_mockClient.Object);
        }

        private PaginatedCandidateResponse FakeResponse()
        {
            return new PaginatedCandidateResponse
            {
                Data = new List<CandidateDto>()
            };
        }

        // =====================================================
        // 1️⃣ BY SKILL
        // =====================================================

        [Test]
        public async Task GetBySkill_ShouldReturnBadRequest_WhenSkillNull()
        {
            var result = await _controller.GetBySkill(null!, 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetBySkill_ShouldReturnBadRequest_WhenSkillWhitespace()
        {
            var result = await _controller.GetBySkill("   ", 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetBySkill_ShouldReturnBadRequest_WhenPageInvalid()
        {
            var result = await _controller.GetBySkill("C#", 0, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetBySkill_ShouldReturnBadRequest_WhenPageSizeTooSmall()
        {
            var result = await _controller.GetBySkill("C#", 1, 0);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetBySkill_ShouldReturnBadRequest_WhenPageSizeTooLarge()
        {
            var result = await _controller.GetBySkill("C#", 1, 500);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetBySkill_ShouldReturnOk_WhenValid()
        {
            _mockClient.Setup(x => x.SearchCandidatesAsync(
                0, int.MaxValue, "C#", null, null, null, 1, 50))
                .ReturnsAsync(FakeResponse());

            var result = await _controller.GetBySkill("C#", 1, 50);

            result.Should().BeOfType<OkObjectResult>();
        }

        // =====================================================
        // 2️⃣ BY EXPERIENCE
        // =====================================================

        [Test]
        public async Task GetByExperience_ShouldReturnBadRequest_WhenNegative()
        {
            var result = await _controller.GetByExperience(-1, 5, 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByExperience_ShouldReturnBadRequest_WhenMinGreaterThanMax()
        {
            var result = await _controller.GetByExperience(10, 5, 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByExperience_ShouldReturnBadRequest_WhenPageInvalid()
        {
            var result = await _controller.GetByExperience(1, 5, 0, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByExperience_ShouldReturnBadRequest_WhenPageSizeInvalid()
        {
            var result = await _controller.GetByExperience(1, 5, 1, 300);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByExperience_ShouldReturnOk_WhenValid()
        {
            _mockClient.Setup(x => x.SearchCandidatesAsync(
                1, 5, null, null, null, null, 1, 50))
                .ReturnsAsync(FakeResponse());

            var result = await _controller.GetByExperience(1, 5, 1, 50);

            result.Should().BeOfType<OkObjectResult>();
        }

        // =====================================================
        // 3️⃣ BY AVAILABILITY
        // =====================================================

        [Test]
        public async Task GetByAvailability_ShouldReturnBadRequest_WhenStartAfterEnd()
        {
            var result = await _controller.GetByAvailability(
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow,
                1,
                50);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByAvailability_ShouldReturnBadRequest_WhenPageInvalid()
        {
            var result = await _controller.GetByAvailability(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                0,
                50);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByAvailability_ShouldReturnBadRequest_WhenPageSizeInvalid()
        {
            var result = await _controller.GetByAvailability(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                1,
                500);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByAvailability_ShouldReturnOk_WhenValid()
        {
            _mockClient.Setup(x => x.SearchCandidatesAsync(
                0,
                int.MaxValue,
                null,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                null,
                1,
                50))
                .ReturnsAsync(FakeResponse());

            var result = await _controller.GetByAvailability(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                1,
                50);

            result.Should().BeOfType<OkObjectResult>();
        }

        // =====================================================
        // 4️⃣ BY PRIMARY SKILL LEVEL
        // =====================================================

        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnBadRequest_WhenNull()
        {
            var result = await _controller.GetByPrimarySkillLevel(null!, 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnBadRequest_WhenWhitespace()
        {
            var result = await _controller.GetByPrimarySkillLevel("  ", 1, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnBadRequest_WhenPageInvalid()
        {
            var result = await _controller.GetByPrimarySkillLevel("P1", 0, 50);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnBadRequest_WhenPageSizeInvalid()
        {
            var result = await _controller.GetByPrimarySkillLevel("P1", 1, 300);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetByPrimarySkillLevel_ShouldReturnOk_WhenValid()
        {
            _mockClient.Setup(x => x.SearchCandidatesAsync(
                0,
                int.MaxValue,
                null,
                null,
                null,
                "P1",
                1,
                50))
                .ReturnsAsync(FakeResponse());

            var result = await _controller.GetByPrimarySkillLevel("P1", 1, 50);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}