using CandidateService.Models;
using CandidateService.Services;
using CandidateService.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CandidateService.Tests.Services


{
    public class CandidateBulkInsertServiceTests
    {
        [Test]
        [ExcludeFromCodeCoverage]
        public async Task InsertSingleAsync_WhenCandidateIsNew_InsertsSuccessfully()
        {
            var context = DbContextFactory.Create(nameof(InsertSingleAsync_WhenCandidateIsNew_InsertsSuccessfully));
            var service = new CandidateBulkInsertService(context);

            var candidate = new Candidate
            {
                Name = "John",
                MailId = "john@test.com",
                SkillSet = "C#",
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P2"
            };

            var result = await service.InsertSingleAsync(candidate);

            Assert.That(result, Is.True);
            Assert.That(context.Candidates.CountAsync().Result, Is.EqualTo(1));
        }

        [Test]
        [ExcludeFromCodeCoverage]
        public async Task InsertSingleAsync_WhenDuplicateExists_ReturnsFalse()
        {
            var context = DbContextFactory.Create(nameof(InsertSingleAsync_WhenDuplicateExists_ReturnsFalse));
            var service = new CandidateBulkInsertService(context);

            var candidate = new Candidate
            {
                Name = "John",
                MailId = "john@test.com",
                SkillSet = "C#",
                ExperienceMonths = 10,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P2"
            };

            await service.InsertSingleAsync(candidate);

            var duplicate = new Candidate
            {
                Name = "John 2",
                MailId = "JOHN@test.com", // case-insensitive
                SkillSet = "c#",
                ExperienceMonths = 20,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P3"
            };

            var result = await service.InsertSingleAsync(duplicate);

            Assert.That(result, Is.False);
            Assert.That(context.Candidates.CountAsync().Result, Is.EqualTo(1));
        }

        [Test]
        [ExcludeFromCodeCoverage]
        public async Task GetExistingKeysAsync_WhenMatchExists_ReturnsKey()
        {
            var context = DbContextFactory.Create(nameof(GetExistingKeysAsync_WhenMatchExists_ReturnsKey));
            var service = new CandidateBulkInsertService(context);

            context.Candidates.Add(new Candidate
            {
                Name = "Jane",
                MailId = "jane@test.com",
                SkillSet = "Python",
                AvailabilityDate = DateTime.Today
            });

            await context.SaveChangesAsync();

            var incoming = new List<Candidate>
            {
                new Candidate
                {
                    MailId = "JANE@test.com",
                    SkillSet = "python",
                    AvailabilityDate = DateTime.Today
                }
            };

            var keys = await service.GetExistingKeysAsync(incoming);

            Assert.That(keys.Count, Is.EqualTo(1));
        }

        [Test]
        [ExcludeFromCodeCoverage]
        public async Task GetExistingKeysAsync_WhenNoMatch_ReturnsEmptySet()
        {
            var context = DbContextFactory.Create(nameof(GetExistingKeysAsync_WhenNoMatch_ReturnsEmptySet));
            var service = new CandidateBulkInsertService(context);

            var keys = await service.GetExistingKeysAsync(new[]
            {
                new Candidate
                {
                    MailId = "none@test.com",
                    SkillSet = "Go",
                    AvailabilityDate = DateTime.Today
                }
            });

            Assert.That(keys, Is.Empty);
        }
    }
}
