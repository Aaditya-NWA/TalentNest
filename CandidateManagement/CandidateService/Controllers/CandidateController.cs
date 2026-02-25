using CandidateService.Data;
using CandidateService.DTOs;
using CandidateService.Models;
using CandidateService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CandidateService.Controllers
{
    [ApiController]
    [Route("api/candidates")]
    public class CandidateController : ControllerBase
    {
        private readonly CandidateDbContext _context;
        private readonly ICandidateBulkInsertService _service;

        public CandidateController(
              CandidateDbContext context,
              ICandidateBulkInsertService service)
        {
            _context = context;
            _service = service;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidateRequest request)
        {
            var errors = CandidateValidationService.Validate(request);

            if (errors.Any())
                return BadRequest(new { errors });

            var candidate = new Candidate
            {
                Name = request.Name,
                MailId = request.MailId,
                SkillSet = request.SkillSet,
                ExperienceMonths = request.ExperienceMonths,
                AvailabilityDate = request.AvailabilityDate,
                PrimarySkillLevel = request.PrimarySkillLevel
            };

            var inserted = await _service.InsertSingleAsync(candidate);

            if (!inserted)
                return Conflict("Duplicate candidate (MailId + SkillSet + AvailabilityDate)");

            return Ok(candidate);
        }


        // READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be negative");
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }

        // READ ALL (PAGINATED)
        [HttpGet]

        public async Task<IActionResult> GetAll(

            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var totalCount = await _context.Candidates.CountAsync();

            var candidates = await _context.Candidates
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = candidates,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpPost("bulk")]
        [Consumes("multipart/form-data")]
        [ExcludeFromCodeCoverage]
        public async Task<IActionResult> BulkCreate([FromForm] BulkCandidateUploadForm form)
        {
            var file = form.File;

            if (file == null || file.Length == 0)
                return BadRequest("File is missing.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            List<CreateCandidateRequest> dtoList;

            try
            {
                using var stream = file.OpenReadStream();

                dtoList = extension switch
                {
                    ".json" => await ParseJsonAsync(stream),
                    ".csv" => ParseCsv(stream),
                    _ => throw new InvalidOperationException(
                                    "Unsupported file type. Only .json and .csv are allowed.")
                };
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid file content: {ex.Message}");
            }

            if (!dtoList.Any())
                return BadRequest("No candidates found in file.");

            // Reuse existing logic (unchanged)
            //var candidates = dtoList.Select(c => new Candidate
            //{
            //    Name = c.Name,
            //    MailId = c.MailId,
            //    SkillSet = c.SkillSet,
            //    ExperienceMonths = c.ExperienceMonths,
            //    AvailabilityDate = c.AvailabilityDate,
            //    PrimarySkillLevel = c.PrimarySkillLevel
            //}).ToList();
            var validationErrors = new List<string>();
            var rowNumber = 1;

            foreach (var dto in dtoList)
            {
                validationErrors.AddRange(
                    CandidateValidationService.Validate(dto, rowNumber)
                );
                rowNumber++;
            }

            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    message = "Validation failed for uploaded file.",
                    errors = validationErrors
                });
            }

            var candidates = MapCandidates(dtoList);

            var sw = Stopwatch.StartNew();
            var result = await _service.BulkInsertAsync(candidates);
            sw.Stop();

            if (result.skipped == 0)
            {
                return Ok(new
                {
                    totalReceived = candidates.Count,
                    inserted = result.inserted,
                    skipped = result.skipped,
                    timeTakenMs = result.timeTakenMs

                });
            }

            return Ok(new
            {
                totalReceived = candidates.Count,
                inserted = result.inserted,
                skipped = result.skipped,
                timeTakenMs = result.timeTakenMs,
                reasonForSkip = "Duplicate MailId + SkillSet + AvailabilityDate"
            });
        }

        // UPDATE

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCandidateRequest request)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be negative");
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return NotFound();

            if (request.ExperienceMonths <= 0)
            {
                return BadRequest("ExperienceMonths cannot be less than or equal to zero.)");
            }
            ;

            // DUPLICATE VALIDATION (ADD HERE)
            var tempCandidate = new Candidate
            {
                MailId = request.MailId,
                SkillSet = request.SkillSet,
                AvailabilityDate = request.AvailabilityDate
            };

            var exists = await _service.GetExistingKeysAsync(new[] { tempCandidate });

            // If duplicate exists and it is not the same record
            if (exists.Any() &&
                (candidate.MailId != request.MailId ||
                 candidate.SkillSet != request.SkillSet ||
                 candidate.AvailabilityDate.Date != request.AvailabilityDate.Date))
            {
                return Conflict("Duplicate candidate (MailId + SkillSet + AvailabilityDate)");
            }
           
            candidate.Name = request.Name;
            candidate.MailId = request.MailId;
            candidate.SkillSet = request.SkillSet;
            candidate.ExperienceMonths = request.ExperienceMonths;
            candidate.AvailabilityDate = request.AvailabilityDate;
            candidate.PrimarySkillLevel = request.PrimarySkillLevel;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE
        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be negative");
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return NotFound();

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [ExcludeFromCodeCoverage]
        internal static List<Candidate> MapCandidates(IEnumerable<CreateCandidateRequest> dtoList)
        {
            var list = new List<Candidate>();

            foreach (var c in dtoList)
            {
                list.Add(new Candidate
                {
                    Name = c.Name,
                    MailId = c.MailId,
                    SkillSet = c.SkillSet,
                    ExperienceMonths = c.ExperienceMonths,
                    AvailabilityDate = c.AvailabilityDate,
                    PrimarySkillLevel = c.PrimarySkillLevel
                });
            }

            return list;
        }

        [ExcludeFromCodeCoverage]
        private static async Task<List<CreateCandidateRequest>> ParseJsonAsync(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<List<CreateCandidateRequest>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<CreateCandidateRequest>();
        }

        [ExcludeFromCodeCoverage]
        private static List<CreateCandidateRequest> ParseCsv(Stream stream)
        {
            var result = new List<CreateCandidateRequest>();

            using var reader = new StreamReader(stream);
            var headerLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(headerLine))
                return result;

            var headers = headerLine.Split(',')
                                    .Select(h => h.Trim().ToLowerInvariant())
                                    .ToArray();

            var rowNumber = 1;

            while (!reader.EndOfStream)
            {
                rowNumber++;
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(',');

                var row = headers
                    .Select((h, i) => new { h, v = values.ElementAtOrDefault(i)?.Trim() })
                    .ToDictionary(x => x.h, x => x.v);

                if (!int.TryParse(row.GetValueOrDefault("experiencemonths"), out var exp))
                    throw new InvalidOperationException($"Row {rowNumber}: ExperienceMonths is invalid.");

                if (!DateTime.TryParse(row.GetValueOrDefault("availabilitydate"), out var date))
                    throw new InvalidOperationException($"Row {rowNumber}: AvailabilityDate is invalid.");

                result.Add(new CreateCandidateRequest
                {
                    Name = row.GetValueOrDefault("name") ?? string.Empty,
                    MailId = row.GetValueOrDefault("mailid") ?? string.Empty,
                    SkillSet = row.GetValueOrDefault("skillset") ?? string.Empty,
                    ExperienceMonths = exp,
                    AvailabilityDate = date,
                    PrimarySkillLevel = row.GetValueOrDefault("primaryskilllevel") ?? "P0"
                });
            }

            return result;
        }
        [ExcludeFromCodeCoverage]
        // PAGINATED SEARCH (10k+ Optimized)
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int? minExp,
            [FromQuery] int? maxExp,
            [FromQuery] string? skill,
            [FromQuery] string? start,
            [FromQuery] string? end,
            [FromQuery] string? primarySkillLevel,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            int safeMinExp = minExp ?? 0;
            int safeMaxExp = maxExp ?? int.MaxValue;

            if (safeMinExp < 0 || safeMaxExp < 0)
                return BadRequest("Experience cannot be negative");

            if (safeMinExp > safeMaxExp)
                return BadRequest("minExp cannot be greater than maxExp");

            DateTime? parsedStart = null;
            DateTime? parsedEnd = null;

            if (!string.IsNullOrWhiteSpace(start) &&
                DateTime.TryParse(start, out var s))
                parsedStart = s;

            if (!string.IsNullOrWhiteSpace(end) &&
                DateTime.TryParse(end, out var e))
                parsedEnd = e;

            if (parsedStart.HasValue && parsedEnd.HasValue &&
                parsedStart > parsedEnd)
                return BadRequest("Start date cannot be greater than End date");

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _context.Candidates
                .AsNoTracking()
                .AsQueryable();

            query = query.Where(c =>
                c.ExperienceMonths >= safeMinExp &&
                c.ExperienceMonths <= safeMaxExp);

            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.SkillSet, $"%{skill}%"));
            }

            if (parsedStart.HasValue && parsedEnd.HasValue)
            {
                query = query.Where(c =>
                    c.AvailabilityDate >= parsedStart &&
                    c.AvailabilityDate <= parsedEnd);
            }

            if (!string.IsNullOrWhiteSpace(primarySkillLevel))
            {
                query = query.Where(c =>
                    c.PrimarySkillLevel == primarySkillLevel);
            }

            var totalCount = await query.CountAsync();

            var result = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = result,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
    }

}
