using Microsoft.EntityFrameworkCore;
using RequirementService.Clients;
using RequirementService.Contracts.Clients;
using RequirementService.Contracts.Services;
using RequirementService.Data;
using RequirementService.Services;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ============ DATABASE ============
builder.Services.AddDbContext<RequirementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============ SERVICES ============
builder.Services.AddScoped<IRequirementService, RequirementService.Services.RequirementService>();

// ============ API ============
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Requirement Service API", Version = "v1" });
});
builder.Services.AddScoped<IMatchingService, MatchingService>();

builder.Services.AddHttpClient<ICandidateClient, CandidateClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7199"); // CandidateService URL
});
// Wherever your existing services are registered, add:

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "RequirementService:";  // ← note: different prefix than ReportService
});

var app = builder.Build();

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

[ExcludeFromCodeCoverage]
public partial class Program { }