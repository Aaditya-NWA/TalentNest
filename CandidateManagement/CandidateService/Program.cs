using CandidateService.Data;
using CandidateService.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ============ DATABASE ============
builder.Services.AddDbContext<CandidateDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICandidateBulkInsertService, CandidateBulkInsertService>();

// ============ API ============
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============ DB + TABLE AUTO-CREATION ============
await EnsureDatabaseAndTablesAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

static async Task EnsureDatabaseAndTablesAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");

        // ── Step 1: Ensure ServicesDB exists ─────────────────────────────────
        var masterConnStr = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ToString();

        await using (var masterConn = new SqlConnection(masterConnStr))
        {
            await masterConn.OpenAsync();
            await using var cmd = masterConn.CreateCommand();
            cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'ServicesDB')
                BEGIN
                    CREATE DATABASE ServicesDB;
                END";
            await cmd.ExecuteNonQueryAsync();
            logger.LogInformation("ServicesDB ensured.");
        }

        // ── Step 2: Ensure Candidates + CandidateStaging tables exist ─────────
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var tableCmd = conn.CreateCommand();
        tableCmd.CommandText = @"
            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'Candidates'
            )
            BEGIN
                CREATE TABLE [dbo].[Candidates] (
                    [Id]                  INT IDENTITY(1,1)   NOT NULL,
                    [Name]                NVARCHAR(150)       NOT NULL,
                    [MailId]              NVARCHAR(150)       NOT NULL,
                    [SkillSet]            NVARCHAR(250)       NOT NULL,
                    [ExperienceMonths]    INT                 NOT NULL DEFAULT 0,
                    [AvailabilityDate]    DATETIME2           NOT NULL,
                    [PrimarySkillLevel]   NVARCHAR(3)         NOT NULL DEFAULT 'P0',
                    CONSTRAINT [PK_Candidates] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_Candidates_MailId_SkillSet_AvailabilityDate]
                    ON [dbo].[Candidates] ([MailId], [SkillSet], [AvailabilityDate]);
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'CandidateStaging'
            )
            BEGIN
                CREATE TABLE [dbo].[CandidateStaging] (
                    [Id]                  INT IDENTITY(1,1)   NOT NULL,
                    [Name]                NVARCHAR(150)       NOT NULL,
                    [MailId]              NVARCHAR(150)       NOT NULL,
                    [SkillSet]            NVARCHAR(250)       NOT NULL,
                    [ExperienceMonths]    INT                 NOT NULL DEFAULT 0,
                    [AvailabilityDate]    DATETIME2           NOT NULL,
                    [PrimarySkillLevel]   NVARCHAR(3)         NOT NULL DEFAULT 'P0',
                    CONSTRAINT [PK_CandidateStaging] PRIMARY KEY ([Id])
                );
            END";
        await tableCmd.ExecuteNonQueryAsync();
        logger.LogInformation("Candidates and CandidateStaging tables ensured.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to ensure CandidateService database/tables.");
        throw;
    }
}

[ExcludeFromCodeCoverage]
public partial class Program { }