using CandidateService.Data;
using CandidateService.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.Data;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.Repositories;
using InterviewService.Services;
using InterviewService.Validators;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RequirementService.Data;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ============ DATABASES ============
// All 3 point to ServicesDB — InterviewService only CREATES its own tables.
// CandidateDbContext and RequirementDbContext are registered here ONLY because
// InterviewValidationService needs to query Candidates and Requirements tables
// for validation (candidate existence, requirement existence, 6-month rule).
builder.Services.AddDbContext<InterviewDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<CandidateDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<RequirementDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============ REPOSITORIES ============
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// ============ SERVICES ============
builder.Services.AddScoped<IInterviewValidationService, InterviewValidationService>();
builder.Services.AddScoped<IInterviewService, InterviewService.Services.InterviewService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ICandidateBulkInsertService, CandidateBulkInsertService>();

// ============ VALIDATORS ============
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInterviewValidator>();
builder.Services.AddScoped<IValidator<CreateInterviewRequest>, CreateInterviewValidator>();
builder.Services.AddScoped<IValidator<UpdateInterviewRequest>, UpdateInterviewValidator>();
builder.Services.AddScoped<IValidator<CreateFeedbackRequest>, CreateFeedbackValidator>();
builder.Services.AddScoped<IValidator<UpdateFeedbackRequest>, UpdateFeedbackValidator>();
builder.Services.AddScoped<IValidator<SetOutcomeRequest>, SetOutcomeRequestValidator>();

// ============ API ============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Interview Service API", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// ============ DB + TABLE AUTO-CREATION (Interviews + Feedbacks only) ============
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

        // ── Step 2: Ensure Interviews + Feedbacks tables ──────────────────────
        // NOTE: Does NOT touch Candidates or Requirements — those are owned
        // by CandidateService and RequirementService respectively.
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var tableCmd = conn.CreateCommand();
        tableCmd.CommandText = @"
            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'Interviews'
            )
            BEGIN
                CREATE TABLE [dbo].[Interviews] (
                    [Id]                INT IDENTITY(1,1)   NOT NULL,
                    [CandidateId]       INT                 NOT NULL,
                    [RequirementId]     INT                 NOT NULL,
                    [Project]           NVARCHAR(100)       NOT NULL,
                    [Account]           NVARCHAR(100)       NOT NULL,
                    [Interviewer]       NVARCHAR(150)       NOT NULL,
                    [InterviewDate]     DATETIME2           NOT NULL,
                    [Level]             INT                 NOT NULL,
                    [FinalOutcome]      INT                 NOT NULL DEFAULT 0,
                    [DecisionMaker]     INT                 NOT NULL DEFAULT 0,
                    [OutcomeDate]       DATETIME2           NULL,
                    [CreatedAt]         DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedAt]         DATETIME2           NULL,
                    CONSTRAINT [PK_Interviews] PRIMARY KEY ([Id]),
                    CONSTRAINT [CK_Interview_Level]         CHECK ([Level] IN (1, 2)),
                    CONSTRAINT [CK_Interview_Outcome]       CHECK ([FinalOutcome] IN (0, 1, 2)),
                    CONSTRAINT [CK_Interview_DecisionMaker] CHECK ([DecisionMaker] IN (0, 1, 2, 3))
                );

                CREATE INDEX [IX_Interviews_Candidate_Project_Date]
                    ON [dbo].[Interviews] ([CandidateId], [Project], [InterviewDate]);

                CREATE INDEX [IX_Interviews_CandidateId]
                    ON [dbo].[Interviews] ([CandidateId]);

                CREATE INDEX [IX_Interviews_RequirementId]
                    ON [dbo].[Interviews] ([RequirementId]);

                CREATE INDEX [IX_Interviews_InterviewDate]
                    ON [dbo].[Interviews] ([InterviewDate]);
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'Feedbacks'
            )
            BEGIN
                CREATE TABLE [dbo].[Feedbacks] (
                    [Id]                    INT IDENTITY(1,1)   NOT NULL,
                    [InterviewId]           INT                 NOT NULL,
                    [Comments]              NVARCHAR(2000)      NOT NULL,
                    [TechnicalScore]        INT                 NOT NULL,
                    [CommunicationScore]    INT                 NOT NULL,
                    [RecommendedOutcome]    INT                 NOT NULL,
                    [Strengths]             NVARCHAR(500)       NULL,
                    [Weaknesses]            NVARCHAR(500)       NULL,
                    [CreatedAt]             DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
                    [CreatedBy]             NVARCHAR(150)       NOT NULL DEFAULT '',
                    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Feedbacks_Interviews] FOREIGN KEY ([InterviewId])
                        REFERENCES [dbo].[Interviews]([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_Feedbacks_InterviewId]
                    ON [dbo].[Feedbacks] ([InterviewId]);
            END";
        await tableCmd.ExecuteNonQueryAsync();
        logger.LogInformation("Interviews and Feedbacks tables ensured.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to ensure InterviewService database/tables.");
        throw;
    }
}

[ExcludeFromCodeCoverage]
public partial class Program { }