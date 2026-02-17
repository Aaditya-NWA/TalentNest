using CandidateService.DTOs;
using CandidateService.Services;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Tests.Services
{
    public class CandidateValidationServiceTests
    {
        [Test]
        [ExcludeFromCodeCoverage]
        public void Validate_WhenRequestIsValid_ReturnsNoErrors()
        {
            var request = new CreateCandidateRequest
            {
                Name = "John",
                MailId = "john@test.com",
                SkillSet = "C#",
                ExperienceMonths = 24,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P3"
            };

            var errors = CandidateValidationService.Validate(request);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        [ExcludeFromCodeCoverage]
        public void Validate_WhenFieldsAreMissing_ReturnsAllErrors()
        {
            var request = new CreateCandidateRequest
            {
                Name = "",
                MailId = "",
                SkillSet = "",
                ExperienceMonths = -1,
                AvailabilityDate = default,
                PrimarySkillLevel = "X9"
            };

            var errors = CandidateValidationService.Validate(request, rowNumber: 2);

            Assert.That(errors.Count, Is.EqualTo(6));
            Assert.That(errors[0], Does.StartWith("Row 2:"));
        }

        [Test]
        [ExcludeFromCodeCoverage]
        public void Validate_WhenPrimarySkillLevelIsNull_ReturnsError()
        {
            var request = new CreateCandidateRequest
            {
                Name = "A",
                MailId = "a@test.com",
                SkillSet = "Java",
                ExperienceMonths = 1,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = null!
            };

            var errors = CandidateValidationService.Validate(request);

            Assert.That(errors, Has.Exactly(1).Contains("PrimarySkillLevel must be P0–P5."));
        }
    }
}
