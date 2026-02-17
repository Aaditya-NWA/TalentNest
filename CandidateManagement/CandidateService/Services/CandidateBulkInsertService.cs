using CandidateService.Data;
using CandidateService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CandidateService.Services
{
    public class CandidateBulkInsertService : ICandidateBulkInsertService

    {
        private readonly CandidateDbContext _context;

        public CandidateBulkInsertService(CandidateDbContext context)
        {
            _context = context;
        }

        //  SHARED KEY BUILDER
        private static string BuildKey(string mailId, string skillSet, DateTime availabilityDate)
        {
            return $"{mailId.ToLower()}|{skillSet.ToLower()}|{availabilityDate.Date:yyyy-MM-dd}";
        }

        // DUPLICATE CHECK (USED BY SINGLE + BULK) 
        public async Task<HashSet<string>> GetExistingKeysAsync(IEnumerable<Candidate> candidates)
        {
            // Build incoming keys (IN MEMORY – SAFE)
            var incomingKeys = candidates
                .Select(c => BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate))
                .ToHashSet();

            // Pull only required columns from DB (NO string ops in SQL)
            var dbCandidates = await _context.Candidates
                .Select(c => new
                {
                    c.MailId,
                    c.SkillSet,
                    c.AvailabilityDate
                })
                .ToListAsync();

            // Build DB keys in memory
            var existingKeys = dbCandidates
                .Select(c => BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate))
                .Where(k => incomingKeys.Contains(k))
                .ToHashSet();

            return existingKeys;
        }

        //  SINGLE INSERT
        public async Task<bool> InsertSingleAsync(Candidate candidate)
        {
            var existingKeys = await GetExistingKeysAsync(new[] { candidate });

            if (existingKeys.Any())
                return false; // duplicate

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return true;
        }

        //  BULK INSERT (USES SAME DUPLICATION LOGIC) 
        [ExcludeFromCodeCoverage]
        public async Task<(int inserted, int skipped, long timeTakenMs)>
                                       BulkInsertAsync(List<Candidate> candidates)
        {
            if (!candidates.Any())
                return (0, 0, 0);

            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("MailId", typeof(string));
            table.Columns.Add("SkillSet", typeof(string));
            table.Columns.Add("ExperienceMonths", typeof(int));
            table.Columns.Add("AvailabilityDate", typeof(DateTime));
            table.Columns.Add("PrimarySkillLevel", typeof(string));

            foreach (var c in candidates)
            {
                table.Rows.Add(
                    c.Name,
                    c.MailId,
                    c.SkillSet,
                    c.ExperienceMonths,
                    c.AvailabilityDate.Date,
                    c.PrimarySkillLevel
                );
            }

            var connection = (SqlConnection)_context.Database.GetDbConnection();
            await connection.OpenAsync();

            var stopwatch = Stopwatch.StartNew();
            using var transaction = connection.BeginTransaction();


            try
            {
                // 1️⃣ Bulk copy into staging
                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.DestinationTableName = "CandidateStaging";

                    bulkCopy.ColumnMappings.Add("Name", "Name");
                    bulkCopy.ColumnMappings.Add("MailId", "MailId");
                    bulkCopy.ColumnMappings.Add("SkillSet", "SkillSet");
                    bulkCopy.ColumnMappings.Add("ExperienceMonths", "ExperienceMonths");
                    bulkCopy.ColumnMappings.Add("AvailabilityDate", "AvailabilityDate");
                    bulkCopy.ColumnMappings.Add("PrimarySkillLevel", "PrimarySkillLevel");

                    await bulkCopy.WriteToServerAsync(table);
                }

                // 2️⃣ Insert only non-duplicates
                var insertCommand = new SqlCommand(@"
            INSERT INTO Candidates
            (
                Name,
                MailId,
                SkillSet,
                ExperienceMonths,
                AvailabilityDate,
                PrimarySkillLevel
            )
            SELECT
                MIN(s.Name)               AS Name,
                s.MailId,
                s.SkillSet,
                MIN(s.ExperienceMonths)   AS ExperienceMonths,
                s.AvailabilityDate,
                MIN(s.PrimarySkillLevel)  AS PrimarySkillLevel
            FROM CandidateStaging s
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM Candidates c
                WHERE c.MailId = s.MailId
                  AND c.SkillSet = s.SkillSet
                  AND c.AvailabilityDate = s.AvailabilityDate
            )
            GROUP BY
                s.MailId,
                s.SkillSet,
                s.AvailabilityDate;
 

            DECLARE @Inserted INT = @@ROWCOUNT;

            DELETE FROM CandidateStaging;

            SELECT @Inserted;
        ", connection, transaction);

                var inserted = (int)await insertCommand.ExecuteScalarAsync();
                transaction.Commit();
                stopwatch.Stop();
                return (inserted, candidates.Count - inserted, stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                stopwatch.Stop();
                transaction.Rollback();
                throw;
            }
        }


        //public async Task<(int inserted, int skipped)> BulkInsertAsync(List<Candidate> candidates)
        //{
        //    var existingKeys = await GetExistingKeysAsync(candidates);

        //    var newCandidates = candidates
        //        .Where(c => !existingKeys.Contains(
        //            BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate)))
        //        .ToList();

        //    if (!newCandidates.Any())
        //        return (0, candidates.Count);

        //    var table = new DataTable();
        //    table.Columns.Add("Name", typeof(string));
        //    table.Columns.Add("MailId", typeof(string));
        //    table.Columns.Add("SkillSet", typeof(string));
        //    table.Columns.Add("ExperienceMonths", typeof(int));
        //    table.Columns.Add("AvailabilityDate", typeof(DateTime));
        //    table.Columns.Add("PrimarySkillLevel", typeof(string));

        //    foreach (var c in newCandidates)
        //    {
        //        table.Rows.Add(
        //            c.Name,
        //            c.MailId,
        //            c.SkillSet,
        //            c.ExperienceMonths,
        //            c.AvailabilityDate,
        //            c.PrimarySkillLevel
        //        );
        //    }

        //    var connection = (SqlConnection)_context.Database.GetDbConnection();
        //    await connection.OpenAsync();

        //    using var bulkCopy = new SqlBulkCopy(connection)
        //    {
        //        DestinationTableName = "Candidates",
        //        BulkCopyTimeout = 60
        //    };

        //    // EXPLICIT COLUMN MAPPING (MANDATORY)
        //    bulkCopy.ColumnMappings.Add("Name", "Name");
        //    bulkCopy.ColumnMappings.Add("MailId", "MailId");
        //    bulkCopy.ColumnMappings.Add("SkillSet", "SkillSet");
        //    bulkCopy.ColumnMappings.Add("ExperienceMonths", "ExperienceMonths");
        //    bulkCopy.ColumnMappings.Add("AvailabilityDate", "AvailabilityDate");
        //    bulkCopy.ColumnMappings.Add("PrimarySkillLevel", "PrimarySkillLevel");

        //    await bulkCopy.WriteToServerAsync(table);

        //    return (newCandidates.Count, candidates.Count - newCandidates.Count);
        //}
    }
}
