using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RequirementService.Clients;
using RequirementService.Contracts.Clients;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.Services;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ============ DATABASE — only RequirementDbContext ============
builder.Services.AddDbContext<RequirementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============ SERVICES ============
builder.Services.AddScoped<IRequirementService, RequirementService.Services.RequirementService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();

// ============ HTTP CLIENTS ============
builder.Services.AddHttpClient<ICandidateClient, CandidateClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:CandidateService"] ?? "https://localhost:7199");
});

// ============ REDIS ============
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.InstanceName = "RequirementService:";
});

// ============ API ============
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Requirement Service API", Version = "v1" });
});

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

        // ── Step 2: Ensure Requirements table exists ──────────────────────────
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var tableCmd = conn.CreateCommand();
        tableCmd.CommandText = @"
            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'Requirements'
            )
            BEGIN
                CREATE TABLE [dbo].[Requirements] (
                    [Id]                        INT IDENTITY(1,1)   NOT NULL,
                    [Project]                   NVARCHAR(200)       NOT NULL,
                    [SkillsNeeded]              NVARCHAR(500)       NOT NULL,
                    [MinExperienceMonths]        INT                 NOT NULL DEFAULT 0,
                    [MaxExperienceMonths]        INT                 NOT NULL DEFAULT 0,
                    [AvailabilityStart]          DATETIME2           NOT NULL,
                    [AvailabilityEnd]            DATETIME2           NOT NULL,
                    [ClientInterviewRequired]    BIT                 NOT NULL DEFAULT 0,
                    [CreatedAt]                  DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
                    [RequiredPrimarySkillLevel]  NVARCHAR(3)         NOT NULL DEFAULT 'P0',
                    CONSTRAINT [PK_Requirements] PRIMARY KEY ([Id])
                );
            END";
        await tableCmd.ExecuteNonQueryAsync();
        logger.LogInformation("Requirements table ensured.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to ensure RequirementService database/tables.");
        throw;
    }
}

[ExcludeFromCodeCoverage]
public partial class Program { }